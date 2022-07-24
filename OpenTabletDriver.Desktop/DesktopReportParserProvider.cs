using OpenTabletDriver.Components;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Desktop
{
    public class DesktopReportParserProvider : IReportParserProvider
    {
        private readonly IPluginFactory _pluginFactory;

        public DesktopReportParserProvider(IPluginFactory pluginFactory)
        {
            _pluginFactory = pluginFactory;
        }

        public IReportParser<IDeviceReport> GetReportParser(string reportParserName)
        {
            return _pluginFactory.Construct<IReportParser<IDeviceReport>>(reportParserName)!;
        }
    }
}
