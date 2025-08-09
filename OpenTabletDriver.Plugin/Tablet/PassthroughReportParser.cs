namespace OpenTabletDriver.Plugin.Tablet
{
    public class PassthroughReportParser : IReportParser<IDeviceReport>
    {
        public virtual IDeviceReport Parse(byte[] data)
        {
            return new DeviceReport(data);
        }
    }
}
