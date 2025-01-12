using System.Runtime.CompilerServices;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.Graphire
{
    public class GraphireReportParser : IReportParser<IDeviceReport>
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
                    return new GraphireMouseReport(report);
                }

                return new GraphireTabletReport(report);
            }

            return new GraphireAuxReport(report);
        }
    }
}
