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

        public void SetProfile(Profile profile)
        {
            var digitizer = 

            OutputMode.Tablet = new TabletState
            {
                Properties = new TabletConfiguration
                {
                    Specifications = new TabletSpecifications
                    {
                        Digitizer = new DigitizerSpecifications
                        {
                            MaxX = 2000,
                            MaxY = 2000,
                            Width = 20,
                            Height = 20,
                        },
                        Pen = new PenSpecifications
                        {
                            ActiveReportID = new DetectionRange(null, null)
                        }
                    }
                }
            };

            OutputMode.Output = new Area
            {
                Width = profile.DisplayWidth,
                Height = profile.DisplayHeight,
                Position = new Vector2
                {
                    X = profile.DisplayX,
                    Y = profile.DisplayY
                }
            };

            OutputMode.Input = new Area
            {
                Width = profile.TabletWidth,
                Height = profile.TabletHeight,
                Position = new Vector2
                {
                    X = profile.TabletX,
                    Y = profile.TabletY
                },
                Rotation = profile.TabletRotation
            };

            var data = new byte[8];
            var randGen = new Random();
            randGen.NextBytes(data);

            var parser = new TabletReportParser();

            Report = parser.Parse(data);
        }

        [GlobalSetup]
        public void Setup()
        {
            var profile = new Profile()
            {
                DisplayWidth = 1366,
                DisplayHeight = 768,
                DisplayX = 0,
                DisplayY = 0,
                TabletWidth = 20,
                TabletHeight = 20
            };

            SetProfile(profile);
        }

        [Benchmark]
        public void Output()
        {
            OutputMode.Read(Report);
        }
    }
}