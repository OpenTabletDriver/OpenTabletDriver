using System;
using System.Numerics;

namespace OpenTabletDriver.Plugin.Tablet
{
    public interface ITabletReport : IAbsolutePositionReport
    {
        uint Pressure { set; get; }
        bool[] PenButtons { set; get; }
    }
}
