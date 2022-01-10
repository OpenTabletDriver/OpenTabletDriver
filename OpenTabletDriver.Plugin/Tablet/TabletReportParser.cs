namespace OpenTabletDriver.Plugin.Tablet
{
    public class TabletReportParser : IReportParser<IDeviceReport>
    {
        public virtual IDeviceReport Parse(byte[] data)
        {
            return new TabletReport(data);
        }
    }
}
