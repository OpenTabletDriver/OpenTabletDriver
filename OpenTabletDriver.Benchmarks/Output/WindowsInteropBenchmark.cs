using System.Numerics;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Interop.Input.Absolute;
using OpenTabletDriver.Desktop.Interop.Input.Relative;

namespace OpenTabletDriver.Benchmarks.Output
{
    public class WindowsInteropBenchmark
    {
        public WindowsInteropBenchmark()
        {
            var serviceProvider = AppInfo.PluginManager.BuildServiceProvider();
            absolutePointer = serviceProvider.GetRequiredService<WindowsAbsolutePointer>();
            relativePointer = serviceProvider.GetRequiredService<WindowsRelativePointer>();
        }

        private WindowsAbsolutePointer absolutePointer;
        private WindowsRelativePointer relativePointer;

        [Benchmark]
        public void SendInputAbsolute()
        {
            absolutePointer.SetPosition(Vector2.Zero);
        }

        [Benchmark]
        public void SendInputRelative()
        {
            relativePointer.Translate(Vector2.Zero);
        }
    }
}