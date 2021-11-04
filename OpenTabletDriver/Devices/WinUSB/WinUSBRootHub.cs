using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using OpenTabletDriver.Native.Windows.CM;
using OpenTabletDriver.Native.Windows.SetupApiStructs;
using OpenTabletDriver.Native.Windows.USB;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Devices;
using static OpenTabletDriver.Native.Windows.CfgMgr32;
using static OpenTabletDriver.Native.Windows.SetupAPI;

namespace OpenTabletDriver.Devices.WinUSB
{
    [DeviceHub, SupportedPlatform(PluginPlatform.Windows)]
    public class WinUSBRootHub : CriticalFinalizerObject, IDeviceHub
    {
        private static readonly Guid[] _winUsbGuids = new Guid[]
        {
            // Standard WinUSB (Zadig)
            new Guid("{dee824ef-729b-4a0e-9c14-b7117d33a817}"),

            // Huion WinUSB
            new Guid("{62f12d4c-3431-4efd-8dd7-8e9aab18d30c}"),
        };

        private readonly CM_NOTIFY_CALLBACK _callback;
        private readonly GCHandle _callbackPin;
        private List<WinUSBInterface> _oldDevices;
        private List<WinUSBInterface> _currentDevices;
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

            _currentDevices = new List<WinUSBInterface>();

            foreach (var guid in _winUsbGuids)
                EnumerateAllDevicesWithGuid(_currentDevices, guid);
        }

        public event EventHandler<DevicesChangedEventArgs> DevicesChanged;

        public IEnumerable<IDeviceEndpoint> GetDevices()
        {
            return _currentDevices;
        }

        private void Enumerate()
        {
            _oldDevices = _currentDevices;
            _currentDevices = new List<WinUSBInterface>();

            foreach (var guid in _winUsbGuids)
                EnumerateAllDevicesWithGuid(_currentDevices, guid);

            DevicesChanged?.Invoke(this, new DevicesChangedEventArgs(_oldDevices, _currentDevices));
        }

        private static void EnumerateAllDevicesWithGuid(List<WinUSBInterface> list, Guid guid)
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

                    list.Add(new WinUSBInterface(deviceInterfaceDetailData.DevicePath));
                }
            }
            finally
            {
                SetupDiDestroyDeviceInfoList(deviceInfoSet);
            }
        }

        private unsafe int NotificationCallback(IntPtr hNotify, IntPtr Context, CM_NOTIFY_ACTION Action, CM_NOTIFY_EVENT_DATA* EventData, int EventDataSize)
        {
            switch (Action)
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
            foreach (var guid in _winUsbGuids)
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