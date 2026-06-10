using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.XP_Pen
{
    public struct XP_PenDeco03WheelReport : IRelativeWheelReport
    {
        public XP_PenDeco03WheelReport(byte[] report, ref byte previousWheelByte, int wheelIndex = 7)
        {
            Raw = report;

            // The XP-Pen Deco 03 wheel sequence is goofy. To track clockwise vs counterclockwise, the previous
            // report is needed. For example, if report[wheelIndex] is 0xC0, and the previous report has report[wheelIndex]
            // set to 0x40, then the wheel is going clockwise. If however the previous report has report[wheelIndex]
            // set to 0x80, then the wheel is going counterclockwise.
            //
            // Because of this, we essentially need to handle each possible byte seperately. I apologize in advance for
            // anyone looking at this.
            //
            // Perhaps only God knows why on earth the firmware devs for the Deco 03 did it this way.
            switch ((previousWheelByte, report[wheelIndex]))
            {
                case (0x80, 0x00):
                case (0x00, 0x40):
                case (0x40, 0xC0):
                case (0xC0, 0x80):
                    AnalogDeltas = [1];
                    break;

                case (0x40, 0x00):
                case (0xC0, 0x40):
                case (0x80, 0xC0):
                case (0x00, 0x80):
                    AnalogDeltas = [-1];
                    break;

                default:
                    AnalogDeltas = [0];
                    break;
            }

            // Set for the next report
            previousWheelByte = report[wheelIndex];
        }

        public byte[] Raw { get; set; }
        public int[] AnalogDeltas { set; get; }
    }
}
