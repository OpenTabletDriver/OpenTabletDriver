using System;
using System.Numerics;
using OpenTabletDriver.Plugin.Tablet.Touch;

namespace OpenTabletDriver.Vendors.Wacom
{
    public struct IntuosV3TouchReport : ITouchReport
    {
        public IntuosV3TouchReport(byte[] report)
        {
            Raw = report;
            Touches = PrevTouches ?? new TouchPoint[maxPoints];

            for (var i = 0; i < 5; ++i)
            {
                var offset = i << 3;
                var touchID = Raw[2 + offset];
                if (touchID == 0)
                    continue;
                touchID -= 1;
                if (touchID >= maxPoints)
                    continue;
                var touchState = Raw[3 + offset];
                if (touchState == 0)
                    Touches[touchID] = null;
                else
                {
                    Touches[touchID] = new TouchPoint
                    {
                        TouchID = touchID,
                        Position = new Vector2
                        {
                            X = BitConverter.ToUInt16(Raw, 4 + offset),
                            Y = BitConverter.ToUInt16(Raw, 6 + offset),
                        },
                    };
                }
            }
            PrevTouches = (TouchPoint[])Touches.Clone();
        }

        private static TouchPoint[] PrevTouches;
        public const int maxPoints = 16;
        public byte[] Raw { set; get; }
        public TouchPoint[] Touches { set; get; }
        public bool ShouldSerializeTouches() => true;
    }
}
