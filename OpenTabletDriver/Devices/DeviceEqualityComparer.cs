using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HidSharp;

namespace OpenTabletDriver.Devices
{
    public class DeviceEqualityComparer : IEqualityComparer<HidDevice>
    {
        public bool Equals(HidDevice x, HidDevice y)
        {
            return x.DevicePath == y.DevicePath;
        }

        public int GetHashCode([DisallowNull] HidDevice obj)
        {
            return obj.DevicePath.GetHashCode();
        }
    }
}