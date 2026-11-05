using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.XP_Pen
{
    public struct XP_PenAuxReport : IAuxReport, IRelativeWheelReport
    {
        public XP_PenAuxReport(byte[] report, int auxIndex = 2, int wheelIndex = 7)
        {
            Raw = report;

            AuxButtons =
            [
                report[auxIndex].IsBitSet(0),
                report[auxIndex].IsBitSet(1),
                report[auxIndex].IsBitSet(2),
                report[auxIndex].IsBitSet(3),
                report[auxIndex].IsBitSet(4),
                report[auxIndex].IsBitSet(5),
                report[auxIndex].IsBitSet(6),
                report[auxIndex].IsBitSet(7),
                report[auxIndex + 1].IsBitSet(0),
                report[auxIndex + 1].IsBitSet(1),
                report[auxIndex + 1].IsBitSet(2),
                report[auxIndex + 1].IsBitSet(3),
                report[auxIndex + 1].IsBitSet(4),
                report[auxIndex + 1].IsBitSet(5),
                report[auxIndex + 1].IsBitSet(6),
                report[auxIndex + 1].IsBitSet(7),
                report[auxIndex + 2].IsBitSet(0),
                report[auxIndex + 2].IsBitSet(1),
                report[auxIndex + 2].IsBitSet(2),
                report[auxIndex + 2].IsBitSet(3),
            ];

            // 0x01 for 1st wheel clockwise, 0x02 for counterclockwise, verified on XP Pen Artist 13.3 Pro V2
            // 0x10 for 2nd wheel clockwise, 0x20 for counterclockwise, verified on XP Pen Artist 22R Pro
            AnalogDeltas =
            [
                report[wheelIndex].IsBitSet(0) ? 1 : report[wheelIndex].IsBitSet(1) ? -1 : 0,
                report[wheelIndex].IsBitSet(4) ? 1 : report[wheelIndex].IsBitSet(5) ? -1 : 0,
            ];
        }

        public bool[] AuxButtons { set; get; }
        public byte[] Raw { set; get; }
        public int[] AnalogDeltas { get; set; }
    }
}
