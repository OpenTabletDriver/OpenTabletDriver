namespace TabletDriverLib.Tablet
{
    public class TabletReportParser : IDeviceReportParser
    {
        public IDeviceReport Parse(byte[] data)
        {
            return new TabletReport(data);
        }
    }
}