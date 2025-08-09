using System.Linq;
using BenchmarkDotNet.Attributes;
using OpenTabletDriver.SystemDrivers;

namespace OpenTabletDriver.Benchmarks.Misc
{
    public class DriverInfoBenchmark
    {
        [Benchmark]
        public DriverInfo[] GetDriverInfos()
        {
            return DriverInfo.GetDriverInfos().ToArray();
        }
    }
}
