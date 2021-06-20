using System;
using System.Numerics;

namespace OpenTabletDriver.Plugin.Tablet
{
    public interface ITabletReport : IDeviceReport
    {
        Vector2 Position { set; get; }
        uint Pressure { set; get; }
        bool[] PenButtons { set; get; }
    }
}
