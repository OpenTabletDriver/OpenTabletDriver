using System;
using System.Collections.Generic;
using System.Linq;
using HidSharp;

namespace OpenTabletDriver.Devices
{
    public class DevicesChangedEventArgs : EventArgs
    {
        public DevicesChangedEventArgs(IEnumerable<HidDevice> oldList, IEnumerable<HidDevice> newList)
        {
            Previous = oldList;
            Current = newList;
        }

        private DeviceEqualityComparer comparer = new DeviceEqualityComparer();

        public IEnumerable<HidDevice> Previous { get; }
        public IEnumerable<HidDevice> Current { get; }

        public IEnumerable<HidDevice> Additions => Current.Except(Previous, comparer);
        public IEnumerable<HidDevice> Removals => Previous.Except(Current, comparer);
        public bool Any() => Additions.Any() | Removals.Any();
    }
}
