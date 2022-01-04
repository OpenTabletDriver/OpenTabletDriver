namespace OpenTabletDriver.Plugin.Tablet
{
    public class AuxReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] data)
        {
            return new AuxReport(data);
        }
    }
}
