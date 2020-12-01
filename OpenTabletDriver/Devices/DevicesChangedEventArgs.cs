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

        public IEnumerable<HidDevice> Previous { get; }
        public IEnumerable<HidDevice> Current { get; }

        public IEnumerable<HidDevice> Additions => Current.Except(Previous);
        public IEnumerable<HidDevice> Removals => Previous.Except(Current);
    }
}
