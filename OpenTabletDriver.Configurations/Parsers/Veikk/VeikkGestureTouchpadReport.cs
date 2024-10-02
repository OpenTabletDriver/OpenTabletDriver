using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Veikk
{
    public struct VeikkGestureTouchpadReport : IGestureTouchReport
    {
        public VeikkGestureTouchpadReport(byte[] report)
        {
            Raw = report;
            
            TouchGestures = new bool[]
            {
                report[3].IsBitSet(0) && report[2].IsBitSet(0), // Top
                report[3].IsBitSet(3) && report[2].IsBitSet(0), // Right
                report[3].IsBitSet(1) && report[2].IsBitSet(0), // Bottom
                report[3].IsBitSet(2) && report[2].IsBitSet(0), // Left
            };
        }

        public byte[] Raw { get; set; }
        public bool[] TouchGestures { get; set; }
    }
}
