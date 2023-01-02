using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.Intuos4
{
    public struct Intuos4AuxReport : IAuxReport
    {
        public Intuos4AuxReport(byte[] report)
        {
            Raw = report;

            var touchWheelButtonByte = report[2];
            var buttonsByte = report[3];

            AuxButtons = new bool[]
            {
                buttonsByte.IsBitSet(0),
                buttonsByte.IsBitSet(1),
                buttonsByte.IsBitSet(2),
                buttonsByte.IsBitSet(3),

                touchWheelButtonByte.IsBitSet(0),

                buttonsByte.IsBitSet(4),
                buttonsByte.IsBitSet(5),
                buttonsByte.IsBitSet(6),
                buttonsByte.IsBitSet(7),
            };
        }

        public byte[] Raw { set; get; }
        public bool[] AuxButtons { set; get; }
    }
}
