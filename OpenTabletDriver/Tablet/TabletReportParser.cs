using JetBrains.Annotations;

namespace OpenTabletDriver.Tablet
{
    /// <summary>
    /// A report parser that returns a generic <see cref="TabletReport"/>.
    /// </summary>
    [PublicAPI]
    public class TabletReportParser : IReportParser<IDeviceReport>
    {
        public virtual IDeviceReport Parse(byte[] data)
        {
            return new TabletReport(data);
        }
    }
}
