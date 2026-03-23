using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
                var newList = EnumerateDevices();
                var oldList = Interlocked.Exchange(ref _hidDevices, newList);
                var changes = new DevicesChangedEventArgs(oldList, newList);

                if (changes.Changes.Any())
                    DevicesChanged?.Invoke(this, changes);
            };

            _hidDevices = EnumerateDevices();
        }

        private IDeviceEndpoint[] _hidDevices;
        public event EventHandler<DevicesChangedEventArgs>? DevicesChanged;

        public IEnumerable<IDeviceEndpoint> GetDevices()
        {
            return _hidDevices;
        }

        private static IDeviceEndpoint[] EnumerateDevices()
        {
            return DeviceList.Local.GetHidDevices()
                .Select(d => new HidSharpEndpoint(d))
                .OrderBy(d => d.DevicePath)
                .ToArray();
        }
    }
}
