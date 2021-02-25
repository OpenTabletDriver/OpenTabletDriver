using System;
using System.Numerics;

namespace OpenTabletDriver.Plugin.Tablet
{
    public interface ITabletReport : IDeviceReport
    {
        uint ReportID { get; }
        Vector2 Position { get; }
        uint Pressure { get; }
        bool[] PenButtons { get; }
        string IDeviceReport.GetStringFormat() =>
            $"ReportID:{ReportID}, " +
            $"Position:[{Position.X},{Position.Y}], " +
            $"Pressure:{Pressure}, " +
            $"PenButtons:[{String.Join(" ", PenButtons)}]";
    }
}