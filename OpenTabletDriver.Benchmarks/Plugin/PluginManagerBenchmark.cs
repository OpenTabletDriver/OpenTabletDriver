using BenchmarkDotNet.Attributes;
using Moq;
using OpenTabletDriver.Desktop.Interop.AppInfo;
using OpenTabletDriver.Desktop.Reflection;

namespace OpenTabletDriver.Benchmarks.Plugin
{
    [DryJob]
    public class PluginManagerBenchmark
    {
        private PluginManager? _pluginManager;

        [Benchmark]
        public void PluginManagerCtor()
        {
            var appInfo = new Mock<AppInfo>();
            _pluginManager = new PluginManager(appInfo.Object);
        }
    }
}
