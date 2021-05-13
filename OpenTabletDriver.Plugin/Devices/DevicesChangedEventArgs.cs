using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenTabletDriver.Plugin.Devices
{
    public class DevicesChangedEventArgs : EventArgs
    {
        public DevicesChangedEventArgs(IEnumerable<IDeviceEndpoint> oldList, IEnumerable<IDeviceEndpoint> newList)
        {
            Previous = oldList;
            Current = newList;
        }

        public IEnumerable<IDeviceEndpoint> Previous { get; }
        public IEnumerable<IDeviceEndpoint> Current { get; }

        public IEnumerable<IDeviceEndpoint> Additions => Current.Except(Previous);
        public IEnumerable<IDeviceEndpoint> Removals => Previous.Except(Current);
        public bool Any() => Additions.Any() | Removals.Any();
    }
}