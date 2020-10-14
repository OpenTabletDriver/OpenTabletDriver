using System;

namespace OpenTabletDriver.Plugin
{
    [Flags]
    public enum PluginPlatform
    {
        Unknown = 0,
        Windows = 1,
        Linux = 2,
        MacOS = 4
    }
}