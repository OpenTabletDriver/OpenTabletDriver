using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.Parblo
{
    public struct ParbloIntangboSAuxReport : IRelativeWheelReport, IWheelButtonReport, IAuxReport
    {
        public ParbloIntangboSAuxReport(byte[] report)
        {
            Raw = report;
            int wheelDataIndex = 3;
            int auxIndex = 2;
            if (report[auxIndex] != 0 && report[wheelDataIndex] != 0)
            {
                AnalogDeltas =
                [
                    report[wheelDataIndex] switch
                    {
                        1 => 1,
                        2 => -1,
                        _ => 0,
                    },
                ];
                WheelButtons =
                [
                    [
                        report[wheelDataIndex] switch
                        {
                            3 => true,
                            _ => false,
                        },
                    ],
                ];
                AuxButtons = [false, false, false, false, false, false];
            }
            else
            {
                AnalogDeltas = [0];
                WheelButtons =
                [
                    [false],
                ];
                AuxButtons =
                [
                    report[auxIndex].IsBitSet(0),
                    report[auxIndex].IsBitSet(1),
                    report[auxIndex].IsBitSet(2),
                    report[auxIndex].IsBitSet(3),
                    report[auxIndex].IsBitSet(4),
                    report[auxIndex].IsBitSet(5),
                ];
            }
        }

        public byte[] Raw { set; get; }
        public int[] AnalogDeltas { get; set; }
        public bool[][] WheelButtons { get; set; }
        public bool[] AuxButtons { get; set; }
    }
}
