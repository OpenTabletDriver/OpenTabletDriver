using System;
using System.Numerics;
using CommunityToolkit.HighPerformance.Buffers;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.IntuosV1
{
    public class IntuosV1RotationReport
    {
        public static bool Parse(scoped in ReadOnlySpan<byte> report, ref DeviceReport deviceReport, ref uint prevPressure, ref Vector2 prevTilt, ref int prevRotation, ref bool[] prevPenButtons)
        {
            deviceReport.PenPosition = new Vector2
            {
                X = (report[3] | report[2] << 8) << 1 | ((report[9] >> 1) & 1),
                Y = (report[5] | report[4] << 8) << 1 | (report[9] & 1),
            };
            deviceReport.Tilt = prevTilt;
            deviceReport.PenPressure = prevPressure;

            deviceReport.PenButtons = MemoryOwner<bool>.Allocate(prevPenButtons.Length);
            prevPenButtons.CopyTo(deviceReport.PenButtons.Span);

            deviceReport.NearProximity = report[1].IsBitSet(6);
            deviceReport.HoverDistance = report[9];

            deviceReport.Rotation = (report[6] << 2) | ((report[7] & 0xC0) >> 6);
            if (!report[7].IsBitSet(5)) deviceReport.Rotation *= -1;

            prevRotation = deviceReport.Rotation!.Value;
            return true;
        }
    }
}
