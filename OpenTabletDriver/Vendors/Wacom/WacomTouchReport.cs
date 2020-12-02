using System.Numerics;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Touch;

namespace OpenTabletDriver.Vendors.Wacom
{
    public struct WacomTouchReport : ITouchReport
    {
        public WacomTouchReport(byte[] report, WacomTouchReport? previous = null)
        {
            Raw = report;
            Touches = previous?.Touches ?? new TouchPoint[maxPoints];

            var nChunks = Raw[1];
            for (var i = 0; i < nChunks; ++i)
            {
                var offset = i * 8;
                var touchID = Raw[2 + offset];
                if (touchID == 129)
                {
                    ApplyTouchMask((ushort)(Raw[3 + offset] | (Raw[4 + offset] << 8)));
                    return;
                }
                touchID -= 2;
                var touchState = Raw[3 + offset];
                if (touchState == 32)
                {
                    Touches[touchID] = null;
                    continue;
                }
                Touches[touchID] = new TouchPoint
                {
                    TouchID = touchID,
                    Position = new Vector2
                    {
                        X = (Raw[4 + offset] << 4) | (Raw[6 + offset] >> 4),
                        Y = (Raw[5 + offset] << 4) | (Raw[6 + offset] & 0xF)
                    },
                    Pressure = Raw[7 + offset],
                    Confidence = Raw[8 + offset]
                };
            }
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

        public const int maxPoints = 16;

        public byte[] Raw { private set; get; }
        public TouchPoint[] Touches { private set; get; }
    }
}
