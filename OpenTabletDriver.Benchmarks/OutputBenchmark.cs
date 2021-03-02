using BenchmarkDotNet.Attributes;
using OpenTabletDriver.Desktop;

namespace OpenTabletDriver.Benchmarks
{
    public class OutputBenchmark : BaseOutputBenchmark
    {
        [GlobalSetup]
        public void Setup()
        {
            var settings = new Settings()
            {
                DisplayWidth = 1366,
                DisplayHeight = 768,
                DisplayX = 0,
                DisplayY = 0,
                TabletWidth = 20,
                TabletHeight = 20
            };

            base.SetSettings(settings);
        }

        [Benchmark]
        public void Output()
        {
            OutputMode.Read(Report);
        }
    }
}