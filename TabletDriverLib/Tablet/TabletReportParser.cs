namespace TabletDriverLib.Tablet
{
    public class TabletReportParser : ITabletReportParser
    {
        public ITabletReport Parse(byte[] data)
        {
            return new TabletReport(data);
        }
    }
}