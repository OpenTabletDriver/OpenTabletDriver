using System.Numerics;

namespace OpenTabletDriver.Plugin.Tablet.Touch
{
    public class TouchPoint
    {
        public byte TouchID;
        public Vector2 Position;
        public byte Pressure;
        public byte Confidence;
        public override string ToString()
        {
            return $"point #{TouchID}: {Position}; pressure = {Pressure} ({Confidence})";
        }
    }
}
