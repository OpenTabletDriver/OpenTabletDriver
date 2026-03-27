using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Waltop
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class WaltopSiriusReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] data)
        {
            // Report ID 16 (0x10) = pen digitizer report
            if (data[0] == 0x10 && data.Length >= 10)
            {
                bool inRange = (data[1] & 0x10) != 0;

                if (!inRange)
                    return new OutOfRangeReport(data);

                return new WaltopSiriusTabletReport(data);
            }

            return new DeviceReport(data);
        }
    }
}
