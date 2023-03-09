using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.FlooGoo
{
    public class FmaReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] report)
        {
            if (report == null || report.Length == 0)
                return null!; // returning null makes OTD ignore this report
            if (report.Length < 12 || report[0] != 0x01)
                return new DeviceReport(report);

            // FlooGoo FMA is a dongle that passthroughs the report from
            // an apple pencil. There is no hardware support for hover or aux buttons.
            // As a workaround, we use the tip switch to determine if the pen is
            // "in-range".
            return report[1].IsBitSet(0) switch
            {
                true => new FmaTabletReport(report),
                false => new OutOfRangeReport(report)
            };
        }
    }
}
