using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Configurations.Parsers.UCLogic;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Huion
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class GianoReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] data)
        {
            if (data[1] == 0xF1)
                return new KamvasRelWheelReport(data);
            if (data[1] == 0xF0 && data[2] == 0x00 && data[3] == 0x00)
                return new UCLogicAuxRelWheelReport(data);
            if (data[1] == 0xF0 && data[2] == 0x01 && data[3] == 0x01)
                return new UCLogicAuxAbsStripReport(data);
            if (data[1].IsBitSet(5) && data[1].IsBitSet(6))
                return new UCLogicAuxReport(data);
            else
                return new GianoReport(data);
        }
    }
}
