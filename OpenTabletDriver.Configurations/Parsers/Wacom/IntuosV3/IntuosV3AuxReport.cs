using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.IntuosV3
{
    public struct IntuosV3AuxReport : IAuxReport, IRelativeWheelReport
    {
        public IntuosV3AuxReport(byte[] report)
        {
            Raw = report;

            var auxByte = report[1];
            var auxByte2 = report[3];
            AuxButtons = new bool[]
            {
                auxByte.IsBitSet(0),
                auxByte.IsBitSet(1),
                auxByte.IsBitSet(2),
                auxByte.IsBitSet(3),
                auxByte2.IsBitSet(0),
                auxByte.IsBitSet(4),
                auxByte.IsBitSet(5),
                auxByte.IsBitSet(6),
                auxByte.IsBitSet(7),
                auxByte2.IsBitSet(1),
            };

            // Wheel rotation is a signed 7-bit value in report[4] (left wheel)
            // and report[5] (right wheel)
            var wheelByte = report[4];
            if ((wheelByte & 0x7F) != 0)
                Delta = (sbyte)(wheelByte << 1) >> 1;

            // TODO: once multiple wheels are supported by OpenTabletDriver,
            // handle the second wheel separately
            var wheelByte2 = report[5];
            if ((wheelByte2 & 0x7F) != 0)
                Delta = (sbyte)(wheelByte2 << 1) >> 1;
        }

        public byte[] Raw { set; get; }
        public bool[] AuxButtons { set; get; }
        public int? Delta { get; set; }
    }
}
