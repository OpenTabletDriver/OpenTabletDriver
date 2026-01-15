using System;

namespace OpenTabletDriver.Plugin.Platform.Pointer
{
    public interface ISystemPointerFilter
    {
        bool Enabled { get; set; }
    }
}
