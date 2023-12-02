using OpenTabletDriver.Configurations.Parsers.UCLogic;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Huion
{
    public class InspiroyReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] data)
        {
            return data[1] switch
            {
                0xe0 => new UCLogicAuxReport(data),
                // Group buttons, no way to use them properly for now
                0xe3 => new UCLogicAuxReport(data),
                // Wheel data, reported in data[5], ignoring
                0xf1 => new DeviceReport(data),
                _ => new TiltTabletReport(data)
            };
        }
    }
}
