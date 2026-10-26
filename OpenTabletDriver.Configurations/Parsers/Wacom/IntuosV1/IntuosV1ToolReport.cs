using System;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.IntuosV1
{
    public class IntuosV1ToolReport
    {
        public static bool Parse(scoped in ReadOnlySpan<byte> report, ref DeviceReport deviceReport)
        {
            deviceReport.ToolSerial = (ulong)(((report[3] & 0x0f) << 0x1C) +
                (report[4] << 0x14) + (report[5] << 0x0C) +
                (report[6] << 0x04) + (report[7] >> 0x04));

            deviceReport.RawToolID = (uint)((report[2] << 4) | (report[3] >> 4) |
                ((report[7] & 0x0f) << 20) | ((report[8] & 0xf0) << 8));

            deviceReport.ToolType = report[3].IsBitSet(7) ? ToolType.Eraser : ToolType.Pen;

            deviceReport.Eraser = deviceReport.ToolType == ToolType.Eraser;

            deviceReport.NearProximity = report[1].IsBitSet(6);
            deviceReport.HoverDistance = (uint)report[9] >> 2;

            return true;
        }
    }
}
