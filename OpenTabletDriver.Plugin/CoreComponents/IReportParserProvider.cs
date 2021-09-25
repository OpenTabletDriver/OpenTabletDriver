using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin
{
    public interface IReportParserProvider
    {
        IReportParser<IDeviceReport> GetReportParser(string reportParserName);
    }
}