using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace OpenTabletDriver.Devices
{
    /// <summary>
    /// Event triggered by addition or removal of devices.
    /// </summary>
    [PublicAPI]
    public class DevicesChangedEventArgs : EventArgs
    {
        public DevicesChangedEventArgs(IList<IDeviceEndpoint>? oldList, IList<IDeviceEndpoint> newList)
        {
            Previous = oldList;
            Current = newList;
        }

        public IList<IDeviceEndpoint>? Previous { get; }
        public IList<IDeviceEndpoint> Current { get; }

        public IEnumerable<IDeviceEndpoint> Additions => Current.Except(Previous ?? Array.Empty<IDeviceEndpoint>(), Comparer);
        public IEnumerable<IDeviceEndpoint> Removals => Previous?.Except(Current, Comparer) ?? Current;
        public IEnumerable<IDeviceEndpoint> Changes => Additions.Concat(Removals);

        public static readonly IEqualityComparer<IDeviceEndpoint> Comparer = new DeviceEndpointComparer();

        private class DeviceEndpointComparer : IEqualityComparer<IDeviceEndpoint>
        {
            public bool Equals(IDeviceEndpoint? x, IDeviceEndpoint? y) => x?.DevicePath == y?.DevicePath;
            public int GetHashCode(IDeviceEndpoint? obj) => obj?.DevicePath.GetHashCode() ?? 0;
        }
    }
}
