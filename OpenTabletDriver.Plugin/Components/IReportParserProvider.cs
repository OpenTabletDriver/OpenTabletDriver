using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin.Components
{
    public interface IReportParserProvider
    {
        IReportParser<IDeviceReport> GetReportParser(string reportParserName);
    }
}