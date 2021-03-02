using System;
using BenchmarkDotNet.Attributes;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Benchmarks
{
    public class ReportParserBenchmark
    {
        private TabletReportParser parser = new TabletReportParser();
        private byte[] data;

        [GlobalSetup]
        public void Setup()
        {
            data = new byte[8];
            var randGen = new Random();
            randGen.NextBytes(data);
        }

        [Benchmark]
        public void ReportParser()
        {
            parser.Parse(data);
        }
    }
}