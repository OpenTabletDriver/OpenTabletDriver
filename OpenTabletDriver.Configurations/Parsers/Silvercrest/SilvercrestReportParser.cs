using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Configurations.Parsers.Genius;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Silvercrest
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class SilvercrestReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] data)
        {
            return data[0] switch
            {
                0x02 => new GeniusTabletReport(data),
                0x0A => new SilvercrestWheelReport(data),
                _ => new DeviceReport(data)
            };
        }
    }
}
