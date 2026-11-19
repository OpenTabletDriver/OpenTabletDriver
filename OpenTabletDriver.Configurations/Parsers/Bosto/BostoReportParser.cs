using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Configurations.Parsers.UCLogic;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Bosto
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class BostoReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] data)
        {
            return data[0] switch
            {
                0x01 when data[1].IsBitSet(4) => new BostoTabletReport(data),
                0x04 => new UCLogicAuxReport(data),
                _ => new OutOfRangeReport(data)
            };
        }
    }
}
