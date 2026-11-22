using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.UCLogic
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class UCLogicV0ReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] data)
        {
            if (data[1] == 0xC0)
                return new OutOfRangeReport(data);

            if (data[1] == 0xE0 && data[3] == 0x01)
                return new UCLogicAuxReport(data);
            else if (data[1] == 0xE0 && data[3] == 0x10)
                return new UCLogicV1WheelReport(data);
            else
                return new TabletReport(data);
        }
    }
}
