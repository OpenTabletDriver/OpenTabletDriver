using System;
using System.Numerics;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Output;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Benchmarks
{
    public abstract class BaseOutputBenchmark
    {
        public AbsoluteMode OutputMode { get; } = new AbsoluteMode();
        public IDeviceReport Report { get; set; }

        public void SetSettings(Settings settings)
        {
            var digitizer = new DigitizerIdentifier
            {
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

            var data = new byte[8];
            var randGen = new Random();
            randGen.NextBytes(data);

            var parser = new TabletReportParser();

            Report = parser.Parse(data);
        }
    }
}