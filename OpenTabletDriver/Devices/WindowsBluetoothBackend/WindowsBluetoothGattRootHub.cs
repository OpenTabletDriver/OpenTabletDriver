using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Win32.SafeHandles;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Interop;

namespace OpenTabletDriver.Devices.WindowsBluetoothBackend
{
    [DeviceHub, SupportedPlatform(SystemPlatform.Windows)]
    public sealed class WindowsBluetoothGattRootHub : IDeviceHub
    {
        private static readonly Guid WH851PenDataService = Guid.Parse("0000FFE0-0000-1000-8000-00805F9B34FB");
        private IDeviceEndpoint[] _endpoints;

        public WindowsBluetoothGattRootHub()
        {
            _endpoints = EnumerateDevices();
        }

        public event EventHandler<DevicesChangedEventArgs>? DevicesChanged;

        public IEnumerable<IDeviceEndpoint> GetDevices()
        {
            var endpoints = EnumerateDevices();
            var changes = new DevicesChangedEventArgs(_endpoints, endpoints);
            _endpoints = endpoints;

            if (changes.Changes.Any())
                DevicesChanged?.Invoke(this, changes);

            return _endpoints;
        }

        private static IDeviceEndpoint[] EnumerateDevices()
        {
            return WindowsBluetoothGattNative.EnumerateDeviceInterfacePaths(WH851PenDataService)
                .Where(IsWH851Path)
                .Select(path => new WindowsBluetoothGattEndpoint(path, WH851PenDataService))
                .OrderBy(endpoint => endpoint.DevicePath)
                .ToArray();
        }

        private static bool IsWH851Path(string path)
        {
            return path.Contains("vid&02256c_pid&8251", StringComparison.OrdinalIgnoreCase)
                || path.Contains("vid_256c&pid_8251", StringComparison.OrdinalIgnoreCase);
        }
    }

    internal sealed class WindowsBluetoothGattEndpoint : IDeviceEndpoint
    {
        public WindowsBluetoothGattEndpoint(string devicePath, Guid serviceUuid)
        {
            DevicePath = devicePath;
            _serviceUuid = serviceUuid;
            SerialNumber = GetBluetoothAddress(devicePath);
        }

        private readonly Guid _serviceUuid;

        public int ProductID => 0x8251;
        public int VendorID => 0x256c;
        public int InputReportLength => 12;
        public int OutputReportLength => 0;
        public int FeatureReportLength => 0;
        public string Manufacturer => "GAOMON";
        public string ProductName => "WH851 Bluetooth";
        public string FriendlyName => "Gaomon WH851 Bluetooth";
        public string? SerialNumber { get; }
        public string DevicePath { get; }
        public bool CanOpen => true;
        public IDictionary<string, string>? DeviceAttributes => new Dictionary<string, string>
        {
            ["BluetoothServiceUuid"] = _serviceUuid.ToString()
        };

        public IDeviceEndpointStream? Open()
        {
            return WindowsBluetoothGattEndpointStream.Open(DevicePath);
        }

        public string? GetDeviceString(byte index)
        {
            return null;
        }

        public bool IsSibling(IDeviceEndpoint other)
        {
            return other is WindowsBluetoothGattEndpoint endpoint
                && string.Equals(SerialNumber, endpoint.SerialNumber, StringComparison.OrdinalIgnoreCase);
        }

        private static string? GetBluetoothAddress(string devicePath)
        {
            var match = Regex.Match(devicePath, @"_([0-9a-f]{12})#", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value : null;
        }
    }

    internal sealed class WindowsBluetoothGattEndpointStream : IDeviceEndpointStream
    {
        private const int S_OK = 0;
        private const int HRESULT_ERROR_MORE_DATA = unchecked((int)0x800700EA);
        private const int ClientCharacteristicConfiguration = 2;
        private const int CharacteristicValueChangedEvent = 0;

        private readonly SafeFileHandle _serviceHandle;
        private readonly BlockingCollection<byte[]> _reports = new();
        private readonly List<IntPtr> _eventHandles = new();
        private readonly WindowsBluetoothGattNative.BluetoothGattEventCallback _callback;
        private bool _disposed;

        private WindowsBluetoothGattEndpointStream(SafeFileHandle serviceHandle)
        {
            _serviceHandle = serviceHandle;
            _callback = HandleGattEvent;
            InitializeNotifications();
        }

        public static IDeviceEndpointStream? Open(string devicePath)
        {
            var handle = WindowsBluetoothGattNative.CreateGattServiceHandle(devicePath);
            if (handle.IsInvalid)
            {
                handle.Dispose();
                return null;
            }

            try
            {
                return new WindowsBluetoothGattEndpointStream(handle);
            }
            catch (Exception ex)
            {
                Log.Exception(ex, LogLevel.Debug);
                handle.Dispose();
                return null;
            }
        }

        public byte[] Read()
        {
            try
            {
                return _reports.Take();
            }
            catch (InvalidOperationException ex)
            {
                throw new ObjectDisposedException(nameof(WindowsBluetoothGattEndpointStream), ex);
            }
        }

        public void Write(byte[] buffer)
        {
        }

        public void GetFeature(byte[] buffer)
        {
        }

        public void SetFeature(byte[] buffer)
        {
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _reports.CompleteAdding();

            foreach (var eventHandle in _eventHandles)
                WindowsBluetoothGattNative.BluetoothGATTUnregisterEvent(eventHandle, 0);

            _eventHandles.Clear();
            _serviceHandle.Dispose();
        }

        private void InitializeNotifications()
        {
            var characteristics = GetCharacteristics();
            var inputCharacteristics = characteristics
                .Where(c => c.IsNotifiable != 0 || c.IsIndicatable != 0)
                .ToArray();

            if (inputCharacteristics.Length == 0)
                throw new IOException("No notifiable WH851 Bluetooth GATT characteristics were found.");

            foreach (var characteristic in inputCharacteristics)
            {
                EnableClientCharacteristicConfiguration(characteristic);
                RegisterValueChanged(characteristic);
                Log.Debug("Bluetooth", $"Registered WH851 GATT characteristic {characteristic.CharacteristicUuid} notifications");
            }
        }

        private WindowsBluetoothGattNative.BTH_LE_GATT_CHARACTERISTIC[] GetCharacteristics()
        {
            var hr = WindowsBluetoothGattNative.BluetoothGATTGetCharacteristics(_serviceHandle, IntPtr.Zero, 0, IntPtr.Zero, out var actual, 0);
            if (hr != HRESULT_ERROR_MORE_DATA && hr != S_OK)
                throw new IOException($"BluetoothGATTGetCharacteristics failed: 0x{hr:X8}");

            if (actual == 0)
                return Array.Empty<WindowsBluetoothGattNative.BTH_LE_GATT_CHARACTERISTIC>();

            var size = Marshal.SizeOf<WindowsBluetoothGattNative.BTH_LE_GATT_CHARACTERISTIC>();
            var buffer = Marshal.AllocHGlobal(size * actual);
            try
            {
                hr = WindowsBluetoothGattNative.BluetoothGATTGetCharacteristics(_serviceHandle, IntPtr.Zero, actual, buffer, out actual, 0);
                if (hr != S_OK)
                    throw new IOException($"BluetoothGATTGetCharacteristics failed: 0x{hr:X8}");

                var characteristics = new WindowsBluetoothGattNative.BTH_LE_GATT_CHARACTERISTIC[actual];
                for (var i = 0; i < actual; i++)
                    characteristics[i] = Marshal.PtrToStructure<WindowsBluetoothGattNative.BTH_LE_GATT_CHARACTERISTIC>(buffer + i * size);

                return characteristics;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private WindowsBluetoothGattNative.BTH_LE_GATT_DESCRIPTOR[] GetDescriptors(ref WindowsBluetoothGattNative.BTH_LE_GATT_CHARACTERISTIC characteristic)
        {
            var hr = WindowsBluetoothGattNative.BluetoothGATTGetDescriptors(_serviceHandle, ref characteristic, 0, IntPtr.Zero, out var actual, 0);
            if (hr != HRESULT_ERROR_MORE_DATA && hr != S_OK)
                return Array.Empty<WindowsBluetoothGattNative.BTH_LE_GATT_DESCRIPTOR>();

            if (actual == 0)
                return Array.Empty<WindowsBluetoothGattNative.BTH_LE_GATT_DESCRIPTOR>();

            var size = Marshal.SizeOf<WindowsBluetoothGattNative.BTH_LE_GATT_DESCRIPTOR>();
            var buffer = Marshal.AllocHGlobal(size * actual);
            try
            {
                hr = WindowsBluetoothGattNative.BluetoothGATTGetDescriptors(_serviceHandle, ref characteristic, actual, buffer, out actual, 0);
                if (hr != S_OK)
                    return Array.Empty<WindowsBluetoothGattNative.BTH_LE_GATT_DESCRIPTOR>();

                var descriptors = new WindowsBluetoothGattNative.BTH_LE_GATT_DESCRIPTOR[actual];
                for (var i = 0; i < actual; i++)
                    descriptors[i] = Marshal.PtrToStructure<WindowsBluetoothGattNative.BTH_LE_GATT_DESCRIPTOR>(buffer + i * size);

                return descriptors;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private void EnableClientCharacteristicConfiguration(WindowsBluetoothGattNative.BTH_LE_GATT_CHARACTERISTIC characteristic)
        {
            var descriptors = GetDescriptors(ref characteristic);
            foreach (var descriptor in descriptors.Where(d => d.DescriptorType == ClientCharacteristicConfiguration))
            {
                var descriptorValue = Marshal.AllocHGlobal(WindowsBluetoothGattNative.BTH_LE_GATT_DESCRIPTOR_VALUE_SIZE);
                try
                {
                    var zero = new byte[WindowsBluetoothGattNative.BTH_LE_GATT_DESCRIPTOR_VALUE_SIZE];
                    Marshal.Copy(zero, 0, descriptorValue, zero.Length);
                    Marshal.WriteInt32(descriptorValue, 0, ClientCharacteristicConfiguration);
                    Marshal.StructureToPtr(descriptor.DescriptorUuid, descriptorValue + 4, false);
                    Marshal.WriteByte(descriptorValue, 24, characteristic.IsNotifiable != 0 ? (byte)1 : (byte)0);
                    Marshal.WriteByte(descriptorValue, 25, characteristic.IsIndicatable != 0 ? (byte)1 : (byte)0);
                    Marshal.WriteInt32(descriptorValue, 72, 0);

                    var mutableDescriptor = descriptor;
                    var hr = WindowsBluetoothGattNative.BluetoothGATTSetDescriptorValue(_serviceHandle, ref mutableDescriptor, descriptorValue, 0);
                    if (hr != S_OK)
                        Log.Debug("Bluetooth", $"BluetoothGATTSetDescriptorValue failed: 0x{hr:X8}");
                }
                finally
                {
                    Marshal.FreeHGlobal(descriptorValue);
                }
            }
        }

        private void RegisterValueChanged(WindowsBluetoothGattNative.BTH_LE_GATT_CHARACTERISTIC characteristic)
        {
            var registrationSize = WindowsBluetoothGattNative.BLUETOOTH_GATT_VALUE_CHANGED_EVENT_REGISTRATION_SIZE;
            var registration = Marshal.AllocHGlobal(registrationSize);
            try
            {
                var zero = new byte[WindowsBluetoothGattNative.BLUETOOTH_GATT_VALUE_CHANGED_EVENT_REGISTRATION_SIZE];
                Marshal.Copy(zero, 0, registration, zero.Length);
                Marshal.WriteInt16(registration, 0, 1);
                Marshal.StructureToPtr(characteristic, registration + 4, false);

                var hr = WindowsBluetoothGattNative.BluetoothGATTRegisterEvent(
                    _serviceHandle,
                    CharacteristicValueChangedEvent,
                    registration,
                    _callback,
                    IntPtr.Zero,
                    out var eventHandle,
                    0
                );

                if (hr != S_OK)
                    throw new IOException($"BluetoothGATTRegisterEvent failed: 0x{hr:X8}");

                _eventHandles.Add(eventHandle);
            }
            finally
            {
                Marshal.FreeHGlobal(registration);
            }
        }

        private void HandleGattEvent(int eventType, IntPtr eventOutParameter, IntPtr context)
        {
            if (_disposed || eventType != CharacteristicValueChangedEvent || eventOutParameter == IntPtr.Zero)
                return;

            try
            {
                var valueChangedEvent = Marshal.PtrToStructure<WindowsBluetoothGattNative.BLUETOOTH_GATT_VALUE_CHANGED_EVENT>(eventOutParameter);
                var valuePointer = valueChangedEvent.CharacteristicValue;
                if (valuePointer == IntPtr.Zero)
                    return;

                var dataSize = Marshal.ReadInt32(valuePointer, 0);
                if (dataSize <= 0 || dataSize > 256)
                    return;

                var report = new byte[dataSize];
                Marshal.Copy(valuePointer + 4, report, 0, dataSize);

                if (!_reports.IsAddingCompleted)
                    _reports.Add(report);
            }
            catch (Exception ex)
            {
                Log.Exception(ex, LogLevel.Debug);
            }
        }
    }

    internal static class WindowsBluetoothGattNative
    {
        public const int BTH_LE_GATT_DESCRIPTOR_VALUE_SIZE = 80;
        public const int BLUETOOTH_GATT_VALUE_CHANGED_EVENT_REGISTRATION_SIZE = 40;

        private const int DIGCF_PRESENT = 2;
        private const int DIGCF_DEVICEINTERFACE = 16;
        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint FILE_SHARE_READ = 1;
        private const uint FILE_SHARE_WRITE = 2;
        private const uint OPEN_EXISTING = 3;

        public delegate void BluetoothGattEventCallback(int eventType, IntPtr eventOutParameter, IntPtr context);

        public static IEnumerable<string> EnumerateDeviceInterfacePaths(Guid interfaceGuid)
        {
            var deviceInfoSet = SetupDiGetClassDevs(in interfaceGuid, IntPtr.Zero, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);
            if (deviceInfoSet == IntPtr.Zero || deviceInfoSet == new IntPtr(-1))
                yield break;

            try
            {
                for (uint index = 0; ; index++)
                {
                    var interfaceData = SP_DEVICE_INTERFACE_DATA.Create();
                    if (!SetupDiEnumDeviceInterfaces(deviceInfoSet, IntPtr.Zero, in interfaceGuid, index, ref interfaceData))
                        yield break;

                    _ = SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref interfaceData, IntPtr.Zero, 0, out var requiredSize, IntPtr.Zero);
                    if (requiredSize == 0)
                        continue;

                    var detailData = Marshal.AllocHGlobal((int)requiredSize);
                    try
                    {
                        Marshal.WriteInt32(detailData, IntPtr.Size == 8 ? 8 : 4 + Marshal.SystemDefaultCharSize);
                        if (SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref interfaceData, detailData, requiredSize, out _, IntPtr.Zero))
                        {
                            var path = Marshal.PtrToStringUni(detailData + 4);
                            if (!string.IsNullOrWhiteSpace(path))
                                yield return path;
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(detailData);
                    }
                }
            }
            finally
            {
                SetupDiDestroyDeviceInfoList(deviceInfoSet);
            }
        }

        public static SafeFileHandle CreateGattServiceHandle(string path)
        {
            return CreateFile(path, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern SafeFileHandle CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern IntPtr SetupDiGetClassDevs(in Guid classGuid, IntPtr enumerator, IntPtr hwndParent, int flags);

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern bool SetupDiEnumDeviceInterfaces(IntPtr deviceInfoSet, IntPtr deviceInfoData, in Guid interfaceClassGuid, uint memberIndex, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr deviceInfoSet, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData, IntPtr deviceInterfaceDetailData, uint deviceInterfaceDetailDataSize, out uint requiredSize, IntPtr deviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern bool SetupDiDestroyDeviceInfoList(IntPtr deviceInfoSet);

        [DllImport("BluetoothAPIs.dll", SetLastError = true)]
        public static extern int BluetoothGATTGetCharacteristics(SafeFileHandle serviceHandle, IntPtr service, ushort characteristicsBufferCount, IntPtr characteristicsBuffer, out ushort characteristicsBufferActual, uint flags);

        [DllImport("BluetoothAPIs.dll", SetLastError = true)]
        public static extern int BluetoothGATTGetDescriptors(SafeFileHandle serviceHandle, ref BTH_LE_GATT_CHARACTERISTIC characteristic, ushort descriptorsBufferCount, IntPtr descriptorsBuffer, out ushort descriptorsBufferActual, uint flags);

        [DllImport("BluetoothAPIs.dll", SetLastError = true)]
        public static extern int BluetoothGATTSetDescriptorValue(SafeFileHandle serviceHandle, ref BTH_LE_GATT_DESCRIPTOR descriptor, IntPtr descriptorValue, uint flags);

        [DllImport("BluetoothAPIs.dll", SetLastError = true)]
        public static extern int BluetoothGATTRegisterEvent(SafeFileHandle serviceHandle, int eventType, IntPtr eventParameterIn, BluetoothGattEventCallback callback, IntPtr callbackContext, out IntPtr eventHandle, uint flags);

        [DllImport("BluetoothAPIs.dll", SetLastError = true)]
        public static extern int BluetoothGATTUnregisterEvent(IntPtr eventHandle, uint flags);

        [StructLayout(LayoutKind.Sequential)]
        private struct SP_DEVICE_INTERFACE_DATA
        {
            public int Size;
            public Guid InterfaceClassGuid;
            public int Flags;
            private readonly UIntPtr _reserved;

            public static SP_DEVICE_INTERFACE_DATA Create()
            {
                return new SP_DEVICE_INTERFACE_DATA { Size = Marshal.SizeOf<SP_DEVICE_INTERFACE_DATA>() };
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BTH_LE_UUID
        {
            public byte IsShortUuid;
            private readonly byte _padding1;
            private readonly byte _padding2;
            private readonly byte _padding3;
            public BTH_LE_UUID_VALUE Value;

            public override string ToString()
            {
                return IsShortUuid != 0 ? $"{Value.ShortUuid:X4}" : Value.LongUuid.ToString();
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct BTH_LE_UUID_VALUE
        {
            [FieldOffset(0)]
            public ushort ShortUuid;

            [FieldOffset(0)]
            public Guid LongUuid;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BTH_LE_GATT_CHARACTERISTIC
        {
            public ushort ServiceHandle;
            public BTH_LE_UUID CharacteristicUuid;
            public ushort AttributeHandle;
            public ushort CharacteristicValueHandle;
            public byte IsBroadcastable;
            public byte IsReadable;
            public byte IsWritable;
            public byte IsWritableWithoutResponse;
            public byte IsSignedWritable;
            public byte IsNotifiable;
            public byte IsIndicatable;
            public byte HasExtendedProperties;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BTH_LE_GATT_DESCRIPTOR
        {
            public ushort ServiceHandle;
            public ushort CharacteristicHandle;
            public int DescriptorType;
            public BTH_LE_UUID DescriptorUuid;
            public ushort AttributeHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BLUETOOTH_GATT_VALUE_CHANGED_EVENT
        {
            public ushort ChangedAttributeHandle;
            private readonly ushort _padding1;
            private readonly uint _padding2;
            public UIntPtr CharacteristicValueDataSize;
            public IntPtr CharacteristicValue;
        }
    }
}
