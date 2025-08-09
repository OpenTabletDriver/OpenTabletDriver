using System;

namespace OpenTabletDriver.Plugin.Tablet
{
    [Flags]
    public enum DeviceVendor
    {
        Wacom = 0x056A,
        Huion = 0x256C,
        Gaomon = 0x256C,
        XP_Pen = 0x28BD,
        VEIKK = 0x2FEB,
        UC_Logic = 0x5543
    }
}
