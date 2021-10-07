using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Vendors.Wacom.IntuosLegacy
{
    public class IntuosLegacyReportParser : IReportParser<IDeviceReport>
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
            if (!report[1].IsBitSet(4))
                return new DeviceReport(report);

            if (report[1].IsBitSet(6))
                return new DeviceReport(report);
                
            return new IntuosLegacyTabletReport(report);
        }
    }
}