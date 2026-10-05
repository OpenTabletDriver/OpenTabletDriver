using System.Numerics;
using BenchmarkDotNet.Attributes;
using NSubstitute;
using OpenTabletDriver.Desktop.Interop.Input.Absolute;
using OpenTabletDriver.Desktop.Interop.Input.Relative;
using OpenTabletDriver.Plugin.Platform.Display;

namespace OpenTabletDriver.Benchmarks.Output
{
    public class WindowsInteropBenchmark
    {
        private WindowsAbsolutePointer absolutePointer = new WindowsAbsolutePointer(Substitute.For<IVirtualScreen>()!);
        private WindowsRelativePointer relativePointer = new WindowsRelativePointer();

        [Benchmark]
        public void SendInputAbsolute()
        {
            absolutePointer.SetPosition(Vector2.Zero);
        }

        [Benchmark]
        public void SendInputRelative()
        {
            relativePointer.SetPosition(Vector2.Zero);
        }
    }
}
