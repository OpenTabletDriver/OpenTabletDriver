using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Tools.udev.Comparers
{
    public class IdentifierComparer : IEqualityComparer<DeviceIdentifier>
    {
        public bool Equals([AllowNull] DeviceIdentifier x, [AllowNull] DeviceIdentifier y)
        {
            return x.VendorID == y.VendorID && x.ProductID == y.ProductID;
        }

        public int GetHashCode([DisallowNull] DeviceIdentifier obj)
        {
            return (obj.VendorID ^ obj.ProductID).GetHashCode();
        }
    }
}
