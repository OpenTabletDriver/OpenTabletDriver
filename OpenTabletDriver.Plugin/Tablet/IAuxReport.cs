using System;

namespace OpenTabletDriver.Plugin.Tablet
{
    public interface IAuxReport : IDeviceReport
    {
        bool[] AuxButtons { get; }
        string IDeviceReport.GetStringFormat() => $"AuxButtons:[{String.Join(" ", AuxButtons)}]";
    }
}