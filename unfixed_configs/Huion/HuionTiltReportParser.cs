using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Configurations.Parsers.UCLogic;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Huion
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class HuionTiltReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] data)
        {
            return data[1] switch
            {
                0xE0 => new InspiroyAuxReport(data),
                0xF0 => new HuionWheelReport(data),
                _ => new TiltTabletReport(data)
            };
        }
    }
}
