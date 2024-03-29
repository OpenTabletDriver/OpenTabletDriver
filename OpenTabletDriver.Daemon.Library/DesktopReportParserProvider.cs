using OpenTabletDriver.Components;
using OpenTabletDriver.Daemon.Reflection;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Daemon
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
