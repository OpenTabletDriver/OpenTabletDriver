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

        public IEnumerable<IDeviceEndpoint> Additions => Current.Except(Previous, Comparer);
        public IEnumerable<IDeviceEndpoint> Removals => Previous.Except(Current, Comparer);
        public IEnumerable<IDeviceEndpoint> Changes => Additions.Concat(Removals);

        public static readonly IEqualityComparer<IDeviceEndpoint> Comparer = new DeviceEndpointComparer();

        private class DeviceEndpointComparer : IEqualityComparer<IDeviceEndpoint>
        {
            public bool Equals(IDeviceEndpoint x, IDeviceEndpoint y) => x?.DevicePath == y?.DevicePath;
            public int GetHashCode(IDeviceEndpoint obj) => obj?.DevicePath?.GetHashCode() ?? 0;
        }
    }
}
