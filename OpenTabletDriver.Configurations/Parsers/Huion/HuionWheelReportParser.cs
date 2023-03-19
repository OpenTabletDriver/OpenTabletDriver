using System.Reflection.Metadata.Ecma335;
using OpenTabletDriver.Configurations.Parsers.UCLogic;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Huion
{
    public class HuionWheelReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] data)
        {
            return data[1] switch
            {
                0xE0 => new UCLogicAuxReport(data),
                0xF0 => new HuionWheelReport(data),
                _ => new TiltTabletReport(data)
            };

        }
    }
}
