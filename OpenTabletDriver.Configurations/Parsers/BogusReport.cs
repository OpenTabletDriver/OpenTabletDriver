using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers
{
    public class BogusReport : IDeviceReport
    {
        public byte[] Raw { get; set; }
    }
}
