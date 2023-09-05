using System.Collections.Generic;

namespace OpenTabletDriver.Tools.udev
{
    public class UdevDeviceData
    {
        public HashSet<string> Names { get; } = new();
        public UdevDeviceFlags Flags { get; set; }
    }
}
