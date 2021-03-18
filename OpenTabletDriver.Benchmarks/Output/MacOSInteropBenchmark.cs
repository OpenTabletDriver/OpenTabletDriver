using System.Numerics;
using BenchmarkDotNet.Attributes;
using OpenTabletDriver.Desktop.Interop.Input.Absolute;
using OpenTabletDriver.Desktop.Interop.Input.Relative;

namespace OpenTabletDriver.Benchmarks.Output
{
    public class MacOSInteropBenchmark
    {
        private MacOSAbsolutePointer absolutePointer = new MacOSAbsolutePointer();
        private MacOSRelativePointer relativePointer = new MacOSRelativePointer();

        [Benchmark]
        public void CoreGraphicsAbsolute()
        {
            absolutePointer.HandlePoint(Vector2.Zero);
        }

        [Benchmark]
        public void CoreGraphicsRelative()
        {
            relativePointer.HandlePoint(Vector2.Zero);
        }
    }
}