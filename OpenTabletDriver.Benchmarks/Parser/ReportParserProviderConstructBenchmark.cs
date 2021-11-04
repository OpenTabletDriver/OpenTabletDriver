using BenchmarkDotNet.Attributes;
using OpenTabletDriver.ComponentProviders;

namespace OpenTabletDriver.Benchmarks.Parser
{
    [DryJob]
    public class ReportParserProviderBenchmark
    {
        public ReportParserProvider ReportParserProvider;

        [Benchmark]
        public void ConstructReportParserProvider()
        {
            ReportParserProvider = new ReportParserProvider();
        }
    }
}