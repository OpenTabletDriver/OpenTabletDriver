using System;
using System.Collections.Generic;
using System.Linq;
using HidSharp;
using JetBrains.Annotations;
using OpenTabletDriver.Attributes;

namespace OpenTabletDriver.Devices.HidSharpBackend
{
    /// <summary>
    /// Device hub implemented with HidSharpCore.
    /// </summary>
    [DeviceHub, PublicAPI]
    public sealed class HidSharpDeviceRootHub : IDeviceHub
    {
        public HidSharpDeviceRootHub()
        {
            DeviceList.Local.Changed += (sender, e) =>
            {
                var newList = DeviceList.Local.GetHidDevices().Select(d => new HidSharpEndpoint(d));
                var changes = new DevicesChangedEventArgs(_hidDevices, newList);
                if (changes.Changes.Any())
                {
                    DevicesChanged?.Invoke(this, changes);
                    _hidDevices = newList;
                }
            };
        }

        private IEnumerable<IDeviceEndpoint> _hidDevices = DeviceList.Local.GetHidDevices().Select(d => new HidSharpEndpoint(d));

        public event EventHandler<DevicesChangedEventArgs>? DevicesChanged;

        public IEnumerable<IDeviceEndpoint> GetDevices()
        {
            return DeviceList.Local.GetHidDevices().Select(d => new HidSharpEndpoint(d));
        }
    }
}
