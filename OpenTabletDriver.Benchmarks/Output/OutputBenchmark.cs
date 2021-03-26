using System;
using System.Numerics;
using BenchmarkDotNet.Attributes;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Benchmarks.Output
{
    public class OutputBenchmark
    {
        public AbsoluteOutputMode OutputMode { get; set; } = new NoopAbsoluteMode();
        public IDeviceReport Report { get; set; }

        public void SetSettings(Settings settings)
        {
            var digitizer = new DigitizerIdentifier
            {
                MaxX = 2000,
                MaxY = 2000,
                Width = 20,
                Height = 20,
                ActiveReportID = new DetectionRange(null, null)
            };

            OutputMode.Tablet = new TabletState(null, digitizer, null);

            OutputMode.Output = new Area
            {
                Width = settings.DisplayWidth,
                Height = settings.DisplayHeight,
                Position = new Vector2
                {
                    X = settings.DisplayX,
                    Y = settings.DisplayY
                }
            };

            OutputMode.Input = new Area
            {
                Width = settings.TabletWidth,
                Height = settings.TabletHeight,
                Position = new Vector2
                {
                    X = settings.TabletX,
                    Y = settings.TabletY
                },
                Rotation = settings.TabletRotation
            };

            OutputMode.Elements = null;

            var data = new byte[8];
            var randGen = new Random();
            randGen.NextBytes(data);

            var parser = new TabletReportParser();

            Report = parser.Parse(data);
        }

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

            SetSettings(settings);
        }

        [Benchmark]
        public void Output()
        {
            OutputMode.Read(Report);
        }
    }
}