using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.Intuos1
{
    public struct Intuos1ToolReport : IToolReport, IProximityReport
    {
        public Intuos1ToolReport(byte[] report, ref byte ToolTypeByte)
        {
            Raw = report;

            Serial = (ulong)(((report[3] & 0x0f) << 0x1C) +
                (report[4] << 0x14) + (report[5] << 0x0C) +
                (report[6] << 0x04) + (report[7] >> 0x04));

            RawToolID = (uint)((report[2] << 4) | (report[3] >> 4) |
                ((report[7] & 0x0f) << 16) | ((report[8] & 0xf0) << 8));

            Tool = report[3].IsBitSet(7) ? ToolType.Eraser : ToolType.Pen;
            ToolTypeByte = report[3];

            NearProximity = report[1].IsBitSet(6);
            HoverDistance = (uint)report[9] >> 2;
        }

        public byte[] Raw { set; get; }
        public ulong Serial { set; get; }
        public uint RawToolID { set; get; }
        public ToolType Tool { set; get; }
        public bool NearProximity { set; get; }
        public uint HoverDistance { set; get; }
    }
}
