using System;

namespace OpenTabletDriver.Plugin.Tablet
{
    public interface IAuxReport : IDeviceReport
    {
        bool[] AuxButtons { set; get; }
    }
}
