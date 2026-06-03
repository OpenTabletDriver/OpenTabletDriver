using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace OpenTabletDriver.Devices.WindowsBluetoothBackend
{
    internal sealed class WH851BluetoothGattEndpointStream : IDeviceEndpointStream
    {
        private const int S_OK = 0;
        private const int HRESULT_ERROR_MORE_DATA = unchecked((int)0x800700EA);
        private const int ClientCharacteristicConfiguration = 2;
        private const int CharacteristicValueChangedEvent = 0;
        private const int MaxQueuedReports = 512;

        private readonly SafeFileHandle _serviceHandle;
        private readonly BlockingCollection<byte[]> _reports = new(new ConcurrentQueue<byte[]>(), MaxQueuedReports);
        private readonly List<IntPtr> _eventHandles = new();
        private readonly WindowsBluetoothGattNative.BluetoothGattEventCallback _callback;
        private bool _disposed;

        private WH851BluetoothGattEndpointStream(SafeFileHandle serviceHandle)
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
                return new WH851BluetoothGattEndpointStream(handle);
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
                throw new ObjectDisposedException(nameof(WH851BluetoothGattEndpointStream), ex);
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
                var descriptorValue = WindowsBluetoothGattNative.BTH_LE_GATT_DESCRIPTOR_VALUE.CreateClientCharacteristicConfiguration(
                    descriptor.DescriptorUuid,
                    characteristic.IsNotifiable != 0,
                    characteristic.IsIndicatable != 0
                );
                var mutableDescriptor = descriptor;
                var hr = WindowsBluetoothGattNative.BluetoothGATTSetDescriptorValue(_serviceHandle, ref mutableDescriptor, ref descriptorValue, 0);
                if (hr != S_OK)
                    Log.Debug("Bluetooth", $"BluetoothGATTSetDescriptorValue failed: 0x{hr:X8}");
            }
        }

        private void RegisterValueChanged(WindowsBluetoothGattNative.BTH_LE_GATT_CHARACTERISTIC characteristic)
        {
            var registration = new WindowsBluetoothGattNative.BLUETOOTH_GATT_VALUE_CHANGED_EVENT_REGISTRATION
            {
                NumCharacteristics = 1,
                Characteristic = characteristic
            };

            var hr = WindowsBluetoothGattNative.BluetoothGATTRegisterEvent(
                _serviceHandle,
                CharacteristicValueChangedEvent,
                ref registration,
                _callback,
                IntPtr.Zero,
                out var eventHandle,
                0
            );

            if (hr != S_OK)
                throw new IOException($"BluetoothGATTRegisterEvent failed: 0x{hr:X8}");

            _eventHandles.Add(eventHandle);
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
                Marshal.Copy(valuePointer + WindowsBluetoothGattNative.BTH_LE_GATT_CHARACTERISTIC_VALUE_DATA_OFFSET, report, 0, dataSize);

                if (!_reports.IsAddingCompleted && !_reports.TryAdd(report))
                    Log.Debug("Bluetooth", "Dropped WH851 Bluetooth report because the input queue is full.");
            }
            catch (Exception ex)
            {
                Log.Exception(ex, LogLevel.Debug);
            }
        }
    }
}



