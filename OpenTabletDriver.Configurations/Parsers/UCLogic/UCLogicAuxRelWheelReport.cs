using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.UCLogic
{
    public struct UCLogicAuxRelWheelReport : IAuxReport, IRelativeWheelReport
    {
        public UCLogicAuxRelWheelReport(byte[] report)
        {
            Raw = report;

            AuxButtons =
            [
                report[4].IsBitSet(0),
                report[4].IsBitSet(1),
                report[4].IsBitSet(2),
                report[4].IsBitSet(3),
                report[4].IsBitSet(4),
                report[4].IsBitSet(5),
                report[4].IsBitSet(6),
                report[4].IsBitSet(7),
            ];

            AnalogDeltas =
            [
                (sbyte)report[5],
                (sbyte)report[6],
            ];
        }

        public bool[] AuxButtons { set; get; }
        public byte[] Raw { set; get; }
        public int[] AnalogDeltas { get; set; }
    }
}
