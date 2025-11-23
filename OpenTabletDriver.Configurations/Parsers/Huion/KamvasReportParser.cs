using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Configurations.Parsers.UCLogic;

namespace OpenTabletDriver.Configurations.Parsers.Huion
{
    public class KamvasReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] data)
        {
            return data[1] switch
            {
                0xE0 => new UCLogicAuxReport(data),
                0xF1 => new KamvasWheelReport(data),
                _ => new TiltTabletReport(data)
            };
        }
    }
}
