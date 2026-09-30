using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.UCLogic
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class UCLogicV2ReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] data)
        {
            return data[1] switch
            {
                0xe0 => new UCLogicAuxReport(data),
                0xf0 => new UCLogicV2WheelReport(data),
                _ => new TiltTabletReport(data)
            };
        }
    }
}
