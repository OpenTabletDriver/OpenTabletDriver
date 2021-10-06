using System.Numerics;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Interop.Input.Absolute;
using OpenTabletDriver.Desktop.Interop.Input.Relative;

namespace OpenTabletDriver.Benchmarks.Output
{
    public class LinuxInteropBenchmark
    {
        public LinuxInteropBenchmark()
        {
            var serviceProvider = AppInfo.PluginManager.BuildServiceProvider();
            absolutePointer = serviceProvider.GetRequiredService<EvdevAbsolutePointer>();
            relativePointer = serviceProvider.GetRequiredService<EvdevRelativePointer>();
        }

        private EvdevAbsolutePointer absolutePointer;
        private EvdevRelativePointer relativePointer;

        [Benchmark]
        public void EvdevAbsolute()
        {
            absolutePointer.SetPosition(Vector2.Zero);
        }

        [Benchmark]
        public void EvdevRelative()
        {
            relativePointer.Translate(Vector2.Zero);
        }
    }
}