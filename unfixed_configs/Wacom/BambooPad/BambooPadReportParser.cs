using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.BambooPad
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class BambooPadReportParser : IReportParser<IDeviceReport>
    {
        public virtual IDeviceReport Parse(byte[] report)
        {
            return report[0] switch
            {
                0x10 => GetToolReport(report),
                _ => new DeviceReport(report)
            };
        }

        private static IDeviceReport GetToolReport(byte[] report)
        {
            if (report[1] == 0x01)
                return new BambooPadTabletReport(report);
            if (report[1] == 0x06)
                return new BambooPadAuxReport(report);

            return new DeviceReport(report);
        }
    }
}
