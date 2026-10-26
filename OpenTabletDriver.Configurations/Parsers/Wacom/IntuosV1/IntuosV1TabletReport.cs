using System;
using System.Numerics;
using CommunityToolkit.HighPerformance.Buffers;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.IntuosV1
{
    public class IntuosV1TabletReport
    {
        public static bool Parse(scoped in ReadOnlySpan<byte> report, ref DeviceReport deviceReport, ref uint prevPressure,
            ref Vector2 prevTilt, in int prevRotation, ref bool[] prevPenButtons)
        {
            var penByte = report[1];

            deviceReport.PenPosition = new Vector2
            {
                X = (report[3] | report[2] << 8) << 1 | ((report[9] >> 1) & 1),
                Y = (report[5] | report[4] << 8) << 1 | (report[9] & 1)
            };

            deviceReport.Tilt = prevTilt = new Vector2
            {
                X = (((report[7] << 1) & 0x7E) | (report[8] >> 7)) - 64,
                Y = (report[8] & 0x7F) - 64
            };

            deviceReport.PenPressure = prevPressure = (uint)((report[6] << 3) | ((report[7] & 0xC0) >> 5) | (report[1] & 1));

            deviceReport.PenButtons = MemoryOwner<bool>.Allocate(2);
            var penButtons = deviceReport.PenButtons.Span;
            penButtons[0] = penByte.IsBitSet(1);
            penButtons[1] = penByte.IsBitSet(2);
            penButtons.CopyTo(prevPenButtons);

            deviceReport.NearProximity = penByte.IsBitSet(6);
            deviceReport.HoverDistance = report[9];
            deviceReport.Rotation = prevRotation;

            return true;
        }
    }
}
