using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Tools.udev.Comparers
{
    public class IdentifierComparer : IEqualityComparer<DeviceIdentifier>
    {
        public bool Equals(DeviceIdentifier? x, DeviceIdentifier? y)
        {
            return x?.VendorID == y?.VendorID && x?.ProductID == y?.ProductID;
        }

        public int GetHashCode(DeviceIdentifier obj)
        {
            return (obj.VendorID ^ obj.ProductID).GetHashCode();
        }
    }
}
