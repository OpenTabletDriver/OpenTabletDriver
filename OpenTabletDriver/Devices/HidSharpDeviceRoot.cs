using System;
using System.Collections.Generic;
using System.Linq;
using HidSharp;
using OpenTabletDriver.Plugin.Devices;

namespace OpenTabletDriver.Devices
{
    public class HidSharpDeviceRootHub : IRootHub
    {
        private HidSharpDeviceRootHub()
        {
            DeviceList.Local.Changed += (sender, e) =>
            {
                var newList = DeviceList.Local.GetHidDevices().Select(d => new HidSharpEndpoint(d));
                var changes = new DevicesChangedEventArgs(hidDevices, newList);
                if (changes.Any())
                {
                    DevicesChanged?.Invoke(this, changes);
                    hidDevices = newList;
                }
            };
        }

        public static IRootHub Current { get; } = new HidSharpDeviceRootHub();

        private IEnumerable<IDeviceEndpoint> hidDevices = DeviceList.Local.GetHidDevices().Select(d => new HidSharpEndpoint(d));

        public event EventHandler<DevicesChangedEventArgs> DevicesChanged;

        public IEnumerable<IDeviceEndpoint> GetDevices()
        {
            return DeviceList.Local.GetHidDevices().Select(d => new HidSharpEndpoint(d));
        }
    }
}