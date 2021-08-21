using System.Runtime.CompilerServices;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Vendors.Wacom.Bamboo
{
    public class BambooReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] report)
        {
            return report[0] switch
            {
                0x02 => GetToolReport(report),
                _ => new DeviceReport(report)
            };
        }

        private IDeviceReport GetToolReport(byte[] report)
        {
            if (Unsafe.ReadUnaligned<ulong>(ref report[1])>0)
                return new BambooTabletReport(report);

            return new BambooAuxReport(report);
        }
    }
}
