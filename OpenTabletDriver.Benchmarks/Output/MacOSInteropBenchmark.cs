using System.Numerics;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Interop.Input.Absolute;
using OpenTabletDriver.Desktop.Interop.Input.Relative;

namespace OpenTabletDriver.Benchmarks.Output
{
    public class MacOSInteropBenchmark
    {
        public MacOSInteropBenchmark()
        {
            var serviceProvider = AppInfo.PluginManager.BuildServiceProvider();
            absolutePointer = serviceProvider.GetRequiredService<MacOSAbsolutePointer>();
            relativePointer = serviceProvider.GetRequiredService<MacOSRelativePointer>();
        }

        private MacOSAbsolutePointer absolutePointer;
        private MacOSRelativePointer relativePointer;

        [Benchmark]
        public void CoreGraphicsAbsolute()
        {
            absolutePointer.SetPosition(Vector2.Zero);
        }

        [Benchmark]
        public void CoreGraphicsRelative()
        {
            relativePointer.Translate(Vector2.Zero);
        }
    }
}