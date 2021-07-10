using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Tablet;
using OpenTabletDriver.Vendors.Wacom.Intuos4;

namespace OpenTabletDriver.Vendors.Wacom.IntuosV1
{
    public class IntuosV1ReportParser : IReportParser<IDeviceReport>
    {
        public virtual IDeviceReport Parse(byte[] report)
        {
            return report[0] switch
            {
                0x02 => GetToolReport(report),
                0x10 => GetToolReport(report),
                0x03 => new IntuosV1AuxReport(report),
                0x0C => new Intuos4AuxReport(report),
                _ => new DeviceReport(report)
            };
        }

        private IDeviceReport GetToolReport(byte[] report)
        {
            if (report[1].IsBitSet(6))
                return new IntuosV1TabletReport(report);
            else if (report[1] == 0xC2)
                return new IntuosV1ToolReport(report);

            return new DeviceReport(report);
        }
    }
}