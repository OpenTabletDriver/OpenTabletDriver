using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Threading;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Devices.WinUSB;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Native.Windows.CM;
using static OpenTabletDriver.Native.Windows.CfgMgr32;

namespace OpenTabletDriver.Devices.WindowsBluetoothBackend
{
    [DeviceHub, SupportedPlatform(SystemPlatform.Windows)]
    public sealed class WH851WindowsBluetoothGattRootHub : CriticalFinalizerObject, IDeviceHub
    {
        internal static readonly Guid PenDataServiceUuid = Guid.Parse("0000FFE0-0000-1000-8000-00805F9B34FB");
        internal const int VendorId = 0x256c;
        internal const int ProductId = 0x8251;

        private readonly CM_NOTIFY_CALLBACK _callback;
        private readonly GCHandle _callbackPin;
        private SafeCmNotificationHandle? _notificationHandle;
        private IDeviceEndpoint[] _currentDevices;

        public unsafe WH851WindowsBluetoothGattRootHub()
        {
            _callback = NotificationCallback;
            _callbackPin = GCHandle.Alloc(_callback);

            if (OperatingSystem.IsWindowsVersionAtLeast(6, 2))
                HookDeviceNotification();
            else
                Log.Write(nameof(WH851WindowsBluetoothGattRootHub), "Bluetooth GATT hotplug notifications require Windows 8 or newer.", LogLevel.Warning);

            _currentDevices = EnumerateDevices();
        }

        public event EventHandler<DevicesChangedEventArgs>? DevicesChanged;

        public IEnumerable<IDeviceEndpoint> GetDevices()
        {
            return _currentDevices;
        }

        private static IDeviceEndpoint[] EnumerateDevices()
        {
            return WindowsBluetoothGattDeviceInterfaceEnumerator.EnumerateDeviceInterfacePaths(PenDataServiceUuid)
                .Where(info => info.HasHardwareId(VendorId, ProductId))
                .Select(info => new WH851BluetoothGattEndpoint(info.DevicePath, PenDataServiceUuid, info.BluetoothAddress))
                .OrderBy(endpoint => endpoint.DevicePath)
                .ToArray();
        }

        private void RefreshDevices()
        {
            var newDevices = EnumerateDevices();
            var oldDevices = Interlocked.Exchange(ref _currentDevices, newDevices);
            var changes = new DevicesChangedEventArgs(oldDevices, newDevices);

            if (changes.Changes.Any())
                DevicesChanged?.Invoke(this, changes);
        }

        private unsafe int NotificationCallback(IntPtr hNotify, IntPtr context, CM_NOTIFY_ACTION action, CM_NOTIFY_EVENT_DATA* eventData, int eventDataSize)
        {
            switch (action)
            {
                case CM_NOTIFY_ACTION.DEVICEINTERFACEARRIVAL:
                case CM_NOTIFY_ACTION.DEVICEINTERFACEREMOVAL:
                case CM_NOTIFY_ACTION.DEVICEREMOVECOMPLETE:
                    RefreshDevices();
                    break;
            }

            return 0;
        }

        private void HookDeviceNotification()
        {
            var notificationFilter = CM_NOTIFY_FILTER.Create(PenDataServiceUuid);
            var result = CM_Register_Notification(in notificationFilter, IntPtr.Zero, _callback, out _notificationHandle);
            if (result != CR.SUCCESS)
                throw new InvalidOperationException($"Failed to register Bluetooth GATT device notifications: {result}");
        }

        ~WH851WindowsBluetoothGattRootHub()
        {
            _notificationHandle?.Dispose();
            _callbackPin.Free();
        }
    }
}



