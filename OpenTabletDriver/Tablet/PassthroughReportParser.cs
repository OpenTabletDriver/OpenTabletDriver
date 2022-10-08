using JetBrains.Annotations;

namespace OpenTabletDriver.Tablet
{
    /// <summary>
    /// A report parser that parses a basic <see cref="DeviceReport"/>.
    /// </summary>
    [PublicAPI]
    public class PassthroughReportParser : IReportParser<IDeviceReport>
    {
        public virtual IDeviceReport Parse(byte[] data)
        {
            return new DeviceReport(data);
        }
    }
}
