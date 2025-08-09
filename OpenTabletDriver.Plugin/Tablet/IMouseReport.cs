using System.Numerics;

namespace OpenTabletDriver.Plugin.Tablet
{
    public interface IMouseReport : IAbsolutePositionReport
    {
        bool[] MouseButtons { set; get; }
        Vector2 Scroll { set; get; }
    }
}
