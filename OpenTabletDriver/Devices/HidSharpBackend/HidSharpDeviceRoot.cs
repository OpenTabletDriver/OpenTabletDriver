using System;
using System.Collections.Generic;
using System.Linq;
using HidSharp;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Devices;

namespace OpenTabletDriver.Devices.HidSharpBackend
{
    [DeviceHub]
    public class HidSharpDeviceRootHub : IDeviceHub
    {
        public HidSharpDeviceRootHub()
        {
            DeviceList.Local.Changed += (_, _) =>
            {
                var newList = DeviceList.Local.GetHidDevices().Select(d => new HidSharpEndpoint(d)).ToArray();
                var changes = new DevicesChangedEventArgs(hidDevices, newList);
                if (changes.Changes.Any())
                {
                    DevicesChanged?.Invoke(this, changes);
                    hidDevices = newList;
                }
            };
        }

        private IEnumerable<IDeviceEndpoint> hidDevices = DeviceList.Local.GetHidDevices().Select(d => new HidSharpEndpoint(d)).ToArray();

        public event EventHandler<DevicesChangedEventArgs>? DevicesChanged;

        public IEnumerable<IDeviceEndpoint> GetDevices()
        {
            return DeviceList.Local.GetHidDevices().Select(d => new HidSharpEndpoint(d));
        }
    }
}
