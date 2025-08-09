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
            absolutePointer.SetPosition(Vector2.Zero);
        }

        [Benchmark]
        public void CoreGraphicsRelative()
        {
            relativePointer.SetPosition(Vector2.Zero);
        }
    }
}
