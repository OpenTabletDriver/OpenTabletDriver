using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers
{
    public class BogusReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] data)
        {
            //TODO: change the API to support not returning anything.
            if (data.Length == 0)
                return null;

            return new DeviceReport(data);
        }
    }
}
