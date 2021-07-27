using System;

namespace OpenTabletDriver.Plugin
{
    [Flags]
    public enum PluginPlatform
    {
        Unknown = 0,
        Windows = 1 << 0,
        Linux = 1 << 1,
        MacOS = 1 << 2,
        FreeBSD = 1 << 3,
        Android = 1 << 4,
        iOS = 1 << 5
    }
}
