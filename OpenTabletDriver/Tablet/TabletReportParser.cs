using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Tablet
{
    public class TabletReportParser : IReportParser<IDeviceReport>
    {
        public virtual IDeviceReport Parse(byte[] data)
        {
            if (data.Length < 12)
                return new TabletReport(data);
            else
                return new TiltTabletReport(data);
        }
    }
}