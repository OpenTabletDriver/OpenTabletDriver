using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.Veikk
{
    public struct VeikkRelativeWheelReport : IRelativeWheelReport
    {
        public VeikkRelativeWheelReport(byte[] report)
        {
            Raw = report;
            if (report[4].IsBitSet(1) && report[3].IsBitSet(0))
            {
                AnalogDeltas = [1];
            }
            else if (report[4].IsBitSet(0) && report[3].IsBitSet(0))
            {
                AnalogDeltas = [-1];
            }
            else
            {
                AnalogDeltas = [0];
            }
        }

        public byte[] Raw { get; set; }
        public int[] AnalogDeltas { get; set; }
    }
}
