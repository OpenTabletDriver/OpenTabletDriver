using System.Runtime.CompilerServices;
using OpenTabletDriver.Tablet;
using OpenTabletDriver.Configurations.Parsers.Wacom.Bamboo;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.BambooV2
{
    // duplicate of BambooReportParser except for the aux handling
    public class BambooV2ReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] report)
        {
            return report[0] switch
            {
                0x02 => GetToolReport(report),
                _ => new DeviceReport(report)
            };
        }

        private static IDeviceReport GetToolReport(byte[] report)
        {
            // If position is available
            if (report[1].IsBitSet(7)
                || Unsafe.ReadUnaligned<uint>(ref report[2]) != 0
                || (report[6] | ((report[7] & 0x03) << 8)) != 0)
            {
                if (report[1].IsBitSet(6))
                {
                    return new BambooMouseReport(report);
                }

                return new BambooTabletReport(report);
            }

            // rel. wheel report on CTE-440: 0x08 up / 0x38 down
            if ((report[7] & 0x8) != 0) // if wheel report
                return new DeviceReport(report);

            return new BambooV2AuxReport(report);
        }
    }
}
