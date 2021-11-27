using System;
using System.Numerics;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Touch;

namespace OpenTabletDriver.Configurations.Parsers.Wacom
{
    public struct WacomTouchReport : ITouchReport, IAuxReport
    {
        public WacomTouchReport(byte[] report, ref TouchPoint[] prevTouches)
        {
            Raw = report;
            AuxButtons = Array.Empty<bool>();
            Touches = prevTouches ?? new TouchPoint[MAX_POINTS];
            if (report[2] == 0x81)
            {
                ApplyTouchMask((ushort)(Raw[3] | (Raw[4] << 8)));
                prevTouches = (TouchPoint[])Touches.Clone();
                return;
            }

            var nChunks = Raw[1];
            for (var i = 0; i < nChunks; i++)
            {
                var offset = (i << 3) + 2;
                var touchID = Raw[offset];
                if (touchID == 0x80)
                {
                    var auxByte = report[1 + offset];
                    AuxButtons = new bool[]
                    {
                        auxByte.IsBitSet(0),
                        auxByte.IsBitSet(1),
                        auxByte.IsBitSet(2),
                        auxByte.IsBitSet(3),
                    };
                    continue;
                }
                touchID -= 2;
                if (touchID >= MAX_POINTS)
                    continue;
                var touchState = Raw[1 + offset];
                if (touchState == 0x20)
                    Touches[touchID] = null;
                else
                {
                    Touches[touchID] = new TouchPoint
                    {
                        TouchID = touchID,
                        Position = new Vector2
                        {
                            X = (Raw[2 + offset] << 4) | (Raw[4 + offset] >> 4),
                            Y = (Raw[3 + offset] << 4) | (Raw[4 + offset] & 0xF)
                        },
                    };
                }
            }
            prevTouches = (TouchPoint[])Touches.Clone();
        }

        private void ApplyTouchMask(ushort mask)
        {
            for (var i = 0; i < MAX_POINTS; i++)
            {
                if ((mask & 1) == 0)
                {
                    Touches[i] = null;
                }
                mask >>= 1;
            }
        }
        public const int MAX_POINTS = 16;
        public byte[] Raw { set; get; }
        public bool[] AuxButtons { set; get; }
        public TouchPoint[] Touches { set; get; }
        public bool ShouldSerializeTouches() => true;
    }
}
