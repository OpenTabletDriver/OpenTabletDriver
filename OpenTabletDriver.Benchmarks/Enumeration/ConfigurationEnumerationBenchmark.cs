using BenchmarkDotNet.Attributes;
using OpenTabletDriver;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Benchmarks
{
    [MemoryDiagnoser]
    public class ConfigurationEnumerationBenchmarks
    {
        public TabletConfiguration Configuration { get; set; }

        [Benchmark]
        public void EnumerateCompiledConfigurations()
        {
            foreach (var config in Configurations.DeviceConfigurationProvider.TabletConfigurations)
            {
                Configuration = config;
            }
        }
    }
}
