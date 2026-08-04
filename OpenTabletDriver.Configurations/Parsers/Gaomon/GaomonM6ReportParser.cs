using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Configurations.Parsers.UCLogic;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Gaomon
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class GaomonM6ReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] data)
        {
            return data[1] switch
            {
                0xe0 => new UCLogicAuxReport(data),
                // 0xf0 is for wheel data, reported in data[5]
                0xf0 => new GaomonM6WheelReport(data),
                _ => new TiltTabletReport(data)
            };
        }
    }
}
