using BenchmarkDotNet.Attributes;
using OpenTabletDriver.Configurations;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Benchmarks
{
    [MemoryDiagnoser]
    public class ConfigurationEnumerationBenchmarks
    {
        private readonly DeviceConfigurationProvider _provider = new DeviceConfigurationProvider();

        public TabletConfiguration? Configuration { get; set; }

        [Benchmark]
        public void EnumerateCompiledConfigurations()
        {
            foreach (var config in _provider.TabletConfigurations)
            {
                Configuration = config;
            }
        }
    }
}
