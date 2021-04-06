using System.Numerics;
using OpenTabletDriver.Plugin.Tablet.Touch;

namespace OpenTabletDriver.Vendors.Wacom
{
    public struct WacomTouchReport : ITouchReport
    {
        public WacomTouchReport(byte[] report)
        {
            Raw = report;
            Touches = PrevTouches ?? new TouchPoint[maxPoints];
            if (report[2] == 0x81)
            {
                ApplyTouchMask((ushort)(Raw[3] | (Raw[4] << 8)));
                PrevTouches = (TouchPoint[])Touches.Clone();
                return;
            }

            var nChunks = Raw[1];
            for (var i = 0; i < nChunks; ++i)
            {
                var offset = i << 3;
                var touchID = Raw[2 + offset];
                touchID -= 2;
                if (touchID >= maxPoints)
                    continue;
                var touchState = Raw[3 + offset];
                if (touchState == 0x20)
                    Touches[touchID] = null;
                else
                {
                    Touches[touchID] = new TouchPoint
                    {
                        TouchID = touchID,
                        Position = new Vector2
                        {
                            X = (Raw[4 + offset] << 4) | (Raw[6 + offset] >> 4),
                            Y = (Raw[5 + offset] << 4) | (Raw[6 + offset] & 0xF)
                        },
                    };
                }
            }
            PrevTouches = (TouchPoint[])Touches.Clone();
        }

        private void ApplyTouchMask(ushort mask)
        {
            for (var i = 0; i < maxPoints; ++i)
            {
                if ((mask & 1) == 0)
                {
                    Touches[i] = null;
                }
                mask >>= 1;
            }
        }
        private static TouchPoint[] PrevTouches;
        public const int maxPoints = 16;
        public byte[] Raw { set; get; }
        public TouchPoint[] Touches { set; get; }
        public bool ShouldSerializeTouches() => true;
    }
}
