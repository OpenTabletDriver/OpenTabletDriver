using System;
using BenchmarkDotNet.Attributes;
using OpenTabletDriver.Tablet;
using OpenTabletDriver.Vendors;

namespace OpenTabletDriver.Benchmarks
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