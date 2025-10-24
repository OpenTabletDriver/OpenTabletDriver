using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.Intuos
{
    public class IntuosReportParser : IReportParser<IDeviceReport>
    {
        public virtual IDeviceReport Parse(byte[] report)
        {
            return report[0] switch
            {
                0x02 => GetToolReport(report),
                _ => new DeviceReport(report)
            };
        }

        private IDeviceReport GetToolReport(byte[] report)
        {
            if (report[1].IsBitSet(6))
                return new IntuosTabletReport(report);

            if (report[1] == 0x80)
                return new OutOfRangeReport(report);

            return new DeviceReport(report);
        }
    }
}
