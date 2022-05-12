using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers
{
    public class BogusReportParser
    {
        public class SkipByteTabletReportParser : IReportParser<IDeviceReport>
        {
            public IDeviceReport Parse(byte[] data)
            {
                return new BogusReport { Raw = data };
            }
        }
    }
}
