using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Threading;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Native.Windows.CM;
using OpenTabletDriver.Native.Windows.SetupApiStructs;
using OpenTabletDriver.Native.Windows.USB;
using static OpenTabletDriver.Native.Windows.CfgMgr32;
using static OpenTabletDriver.Native.Windows.SetupAPI;

namespace OpenTabletDriver.Devices.WinUSB
{
    [DeviceHub, SupportedPlatform(SystemPlatform.Windows)]
    public class WinUSBRootHub : CriticalFinalizerObject, IDeviceHub
    {
        private static readonly Guid[] WinUsbGuids = new Guid[]
        {
            // Standard WinUSB (Zadig)
            new Guid("{dee824ef-729b-4a0e-9c14-b7117d33a817}"),

            // Huion WinUSB
            new Guid("{62f12d4c-3431-4efd-8dd7-8e9aab18d30c}"),
        };

        private readonly CM_NOTIFY_CALLBACK _callback;
        private GCHandle _callbackPin;
        private List<IDeviceEndpoint> _currentDevices;
        private readonly Dictionary<Guid, SafeCmNotificationHandle> _notificationHandles = new();

        public unsafe WinUSBRootHub()
        {
            _callback = NotificationCallback;
            _callbackPin = GCHandle.Alloc(_callback);

            if (OperatingSystem.IsWindowsVersionAtLeast(6, 2))
            {
                HookDeviceNotification();
            }
            else
            {
                Log.Write(nameof(WinUSBRootHub), $"{nameof(WinUSBRootHub)} does not support hotplug functionality for Windows 7 and below.", LogLevel.Warning);
                Log.Write(nameof(WinUSBRootHub), $"WinUSB device connections or disconnections won't be detected automatically.", LogLevel.Warning);
            }

            var newList = new List<IDeviceEndpoint>();

            foreach (var guid in WinUsbGuids)
                EnumerateAllDevicesWithGuid(newList, guid);

            _currentDevices = newList;
        }

        public event EventHandler<DevicesChangedEventArgs>? DevicesChanged;

        public IEnumerable<IDeviceEndpoint> GetDevices()
        {
            return _currentDevices;
        }

        private void Enumerate()
        {
            var newList = new List<IDeviceEndpoint>();
            foreach (var guid in WinUsbGuids)
                EnumerateAllDevicesWithGuid(newList, guid);

            var oldList = Interlocked.Exchange(ref _currentDevices, newList);

            DevicesChanged?.Invoke(this, new DevicesChangedEventArgs(newList, oldList));
        }

        private static void EnumerateAllDevicesWithGuid(List<IDeviceEndpoint> list, Guid guid)
        {
            var deviceInfoSet = SetupDiGetClassDevs(in guid, IntPtr.Zero, IntPtr.Zero, DIGCF.Present | DIGCF.DeviceInterface);

            try
            {
                if (deviceInfoSet == (IntPtr)(-1))
                    throw new WindowsEnumerationException($"Failed to retrieve device info set for {guid}");

                uint i = 0;
                while (true)
                {
                    var deviceInterfaceData = SP_DEVICE_INTERFACE_DATA.Create();
                    var deviceInterfaceDetailData = SP_DEVICE_INTERFACE_DETAIL_DATA.Create();

                    SetupDiEnumDeviceInterfaces(deviceInfoSet, IntPtr.Zero, in guid, i++, ref deviceInterfaceData);
                    if (Marshal.GetLastWin32Error() == ERROR_NO_MORE_ITEMS)
                        break;

                    var size = (uint)Marshal.SizeOf<SP_DEVICE_INTERFACE_DETAIL_DATA>();
                    if (!SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref deviceInterfaceData, ref deviceInterfaceDetailData, size, ref size, IntPtr.Zero))
                        throw new WindowsEnumerationException($"Failed to get device data");

                    TryAdd(list, deviceInterfaceDetailData.DevicePath);
                }
            }
            finally
            {
                SetupDiDestroyDeviceInfoList(deviceInfoSet);
            }
        }

        private static void TryAdd(List<IDeviceEndpoint> list, string devicePath)
        {
            try
            {
                var winUsbInterface = new WinUSBInterface(devicePath);
                list.Add(winUsbInterface);
            }
            catch
            {
                Log.Write("WinUSB", $"Cannot create device for '{devicePath}'");
            }
        }

        private unsafe int NotificationCallback(IntPtr hNotify, IntPtr context, CM_NOTIFY_ACTION action, CM_NOTIFY_EVENT_DATA* eventData, int eventDataSize)
        {
            switch (action)
            {
                case CM_NOTIFY_ACTION.DEVICEINTERFACEARRIVAL:
                case CM_NOTIFY_ACTION.DEVICEINTERFACEREMOVAL:
                    Enumerate();
                    break;
                default:
                    break;
            }
            return 0;
        }

        private void HookDeviceNotification()
        {
            foreach (var guid in WinUsbGuids)
            {
                var notificationFilter = CM_NOTIFY_FILTER.Create(guid);
                var ret = CM_Register_Notification(in notificationFilter, IntPtr.Zero, _callback, out var notificationHandle);

                // Hold notification handles
                _notificationHandles.Add(guid, notificationHandle);

                if (ret != CR.SUCCESS)
                    throw new Exception($"Failed to register for device notifications: {ret}");
            }
        }

        ~WinUSBRootHub()
        {
            foreach (var notificationHandle in _notificationHandles.Values)
                notificationHandle.Dispose();

            _callbackPin.Free();
        }
    }
}
