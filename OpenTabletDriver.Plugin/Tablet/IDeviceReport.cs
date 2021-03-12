using System;

namespace OpenTabletDriver.Plugin.Tablet
{
    public interface IDeviceReport
    {
        byte[] Raw { get; }
    }
}
