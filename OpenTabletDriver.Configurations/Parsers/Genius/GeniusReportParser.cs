using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Genius
{
    public class GeniusReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] data)
        {
            return data[0] switch
            {
                0x10 => new TabletReport(data),
                0x11 => new GeniusMouseReport(data),
                _ => new DeviceReport(data)
            };
        }
    }
}
