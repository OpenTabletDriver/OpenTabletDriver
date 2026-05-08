using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Configurations.Parsers.UCLogic;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Huion
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class HS611ReportParser : IReportParser<IDeviceReport>
    {
        // data[1] value identifying a touch strip (wheel) report.
        private const byte TOUCH_STRIP = 0xF0;

        public IDeviceReport Parse(byte[] data)
        {
            return data[1] switch
            {
                TOUCH_STRIP => new HuionWheelReport(data),
                _ when data[1].IsBitSet(6) => new UCLogicAuxReport(data),
                _ => new TiltTabletReport(data, false, true)
            };
        }
    }
}
