using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Benchmarks.Enumeration
{
    [MemoryDiagnoser]
    public class EnumerationBenchmark
    {
        readonly Driver driver = new Driver();
        Assembly asm;
        TabletConfiguration[] configs;

        [GlobalSetup]
        public void Setup()
        {
            asm = typeof(Driver).Assembly;
            configs = asm.GetManifestResourceNames()
                .Where(path => path.Contains(".json"))
                .Select(path => Serialization.Deserialize<TabletConfiguration>(asm.GetManifestResourceStream(path)))
                .ToArray();
        }

        [Benchmark]
        public void EnumerateWithoutCache()
        {
            var uncachedConfigs = asm.GetManifestResourceNames()
              .Where(path => path.Contains(".json"))
              .Select(path => Serialization.Deserialize<TabletConfiguration>(asm.GetManifestResourceStream(path)))
              .ToArray();

            driver.EnumerateTablets(uncachedConfigs);
        }

        [Benchmark]
        public void EnumerateWithCache()
        {
            driver.EnumerateTablets(configs);
        }
    }
}