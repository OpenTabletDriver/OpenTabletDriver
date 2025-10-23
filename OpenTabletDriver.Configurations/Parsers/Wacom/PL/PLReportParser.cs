using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.PL
{
    public class PLReportParser : IReportParser<IDeviceReport>
    {
        public virtual IDeviceReport Parse(byte[] report)
        {
            // Eraser and Pen Button 2 share a bit
            // Wacom recommends checking if eraser bit is set when pen enters detection to differentiate
            if (!report[1].IsBitSet(6))
            {
                lastReportOutOfRange = true;
                return new OutOfRangeReport(report);
            }
            if (lastReportOutOfRange)
            {
                initialEraser = report[4].IsBitSet(5);
                lastReportOutOfRange = false;
            }

            return new PLTabletReport(report, initialEraser);
        }
        private bool initialEraser;
        private bool lastReportOutOfRange = true;
    }
}
