using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Daemon;
using OpenTabletDriver.Daemon.Contracts;
using OpenTabletDriver.Daemon.Reflection;
using OpenTabletDriver.Output;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Benchmarks.Output
{
    public class OutputBenchmark
    {
        private readonly IPluginFactory _pluginFactory;
        private AbsoluteOutputMode? _outputMode;
        private IDeviceReport? _report;

        public OutputBenchmark()
        {
            var serviceProvider = new DesktopServiceCollection().BuildServiceProvider();
            _pluginFactory = serviceProvider.GetRequiredService<IPluginFactory>();
        }

        private void ApplySettings(PluginSettings settings)
        {
            var config = new TabletConfiguration
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
            };

            var device = new InputDevice(config, null!, null);

            _outputMode = _pluginFactory.Construct<AbsoluteOutputMode>(settings)!;
            _outputMode.Tablet = device;

            var data = new byte[8];
            var randGen = new Random();
            randGen.NextBytes(data);

            var parser = new TabletReportParser();

            _report = parser.Parse(data);
        }

        [GlobalSetup]
        public void Setup()
        {
            var settings = new PluginSettings(typeof(NoopAbsoluteMode), new
            {
                Display = new Area
                {
                    Width = 1366,
                    Height = 768,
                    XPosition = 0,
                    YPosition = 0
                },
                Tablet = new AngledArea
                {
                    Width = 20,
                    Height = 20
                }
            });

            ApplySettings(settings);
        }

        [Benchmark]
        public void Output()
        {
            _outputMode!.Read(_report!);
        }
    }
}
