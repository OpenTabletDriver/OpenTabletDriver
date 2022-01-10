using System;
using BenchmarkDotNet.Attributes;
using OpenTabletDriver.Configurations.Parsers;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Benchmarks.Parser
{
    public class ReportParserBenchmark
    {
        private TabletReportParser parser = new TabletReportParser();
        private SkipByteTabletReportParser skipParser = new SkipByteTabletReportParser();
        private byte[] data;

        [GlobalSetup]
        public void Setup()
        {
            data = new byte[10];
            var randGen = new Random();
            randGen.NextBytes(data);
        }

        [Benchmark]
        public void ReportParser()
        {
            parser.Parse(data);
        }

        [Benchmark]
        public void SkipByteReportParser()
        {
            skipParser.Parse(data);
        }
    }
}
