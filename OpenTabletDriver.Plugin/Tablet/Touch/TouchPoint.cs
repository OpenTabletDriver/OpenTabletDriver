using System.Numerics;

namespace OpenTabletDriver.Plugin.Tablet.Touch
{
    public struct TouchPoint
    {
        public byte TouchID;
        public Vector2 Position;
        public byte Pressure;
        public byte SomeOtherPressure;
        public string AsString
        {
            get { return $"point #{TouchID}: {Position}; pressure = {Pressure} ({SomeOtherPressure})"; }
        }
    }
}
