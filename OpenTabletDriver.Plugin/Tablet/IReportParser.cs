namespace OpenTabletDriver.Plugin.Tablet
{
    public interface IReportParser<T> where T : IDeviceReport
    {
        T Parse(byte[] report);
    }
}
