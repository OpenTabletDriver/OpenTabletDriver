using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.UCLogic
{
    // TODO : Change the used interface to IAbsoluteStripReport once that's implemented
    // TODO : This report has duplicate code with UCLogicAuxRelWheelReport, somehow make them share the aux part
    public struct UCLogicAuxAbsStripReport : IAuxReport, IAbsoluteWheelReport
    {
        public UCLogicAuxAbsStripReport(byte[] report)
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

            var firstStripData = report[5];
            var secondStripData = report[6];

            AnalogPositions =
            [
                firstStripData != 0 ? firstStripData - 1u : null,
                secondStripData != 0 ? secondStripData - 1u : null,
            ];
        }

        public bool[] AuxButtons { set; get; }
        public uint?[] AnalogPositions { set; get; }
        public byte[] Raw { set; get; }
    }
}
