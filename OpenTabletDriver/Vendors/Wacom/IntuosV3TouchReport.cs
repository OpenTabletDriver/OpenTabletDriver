using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Plugin.Tablet.Touch;

namespace OpenTabletDriver.Vendors.Wacom
{
    public struct IntuosV3TouchReport : ITouchReport
    {
        public IntuosV3TouchReport(byte[] report)
        {
            Raw = report;
            Touches = prevTouches ?? new TouchPoint[MAX_POINTS];

            for (var i = 0; i < 5; i++)
            {
                var offset = (i << 3) + 2;
                var touchID = Raw[offset];
                if (touchID == 0)
                    continue;
                touchID -= 1;
                if (touchID >= MAX_POINTS)
                    continue;
                var touchState = Raw[1 + offset];
                if (touchState == 0)
                    Touches[touchID] = null;
                else
                {
                    Touches[touchID] = new TouchPoint
                    {
                        TouchID = touchID,
                        Position = new Vector2
                        {
                            X = Unsafe.ReadUnaligned<ushort>(ref report[2 + offset]),
                            Y = Unsafe.ReadUnaligned<ushort>(ref report[4 + offset]),
                        },
                    };
                }
            }
            prevTouches = (TouchPoint[])Touches.Clone();
        }

        private static TouchPoint[] prevTouches;
        public const int MAX_POINTS = 16;
        public byte[] Raw { set; get; }
        public TouchPoint[] Touches { set; get; }
        public bool ShouldSerializeTouches() => true;
    }
}
