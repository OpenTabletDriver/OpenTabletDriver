using System;
using BenchmarkDotNet.Attributes;
using OpenTabletDriver.Desktop.Profiles;
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
            OutputMode.Tablet = new TabletReference
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
                        Pen = new PenSpecifications()
                    }
                }
            };

            OutputMode.Output = profile.AbsoluteModeSettings.Display.Area;
            OutputMode.Input = profile.AbsoluteModeSettings.Tablet.Area;

            var data = new byte[8];
            var randGen = new Random();
            randGen.NextBytes(data);

            var parser = new TabletReportParser();

            Report = parser.Parse(data);
        }

        [GlobalSetup]
        public void Setup()
        {
            var profile = new Profile
            {
                AbsoluteModeSettings = new AbsoluteModeSettings
                {
                    Display = new AreaSettings
                    {
                        Width = 1366,
                        Height = 768,
                        X = 0,
                        Y = 0,
                    },
                    Tablet = new AreaSettings
                    {
                        Width = 20,
                        Height = 20
                    }
                }
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