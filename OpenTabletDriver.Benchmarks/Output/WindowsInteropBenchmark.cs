using System.Numerics;
using BenchmarkDotNet.Attributes;
using OpenTabletDriver.Desktop.Interop.Input.Absolute;
using OpenTabletDriver.Desktop.Interop.Input.Relative;

namespace OpenTabletDriver.Benchmarks.Output
{
    public class WindowsInteropBenchmark
    {
        private WindowsAbsolutePointer absolutePointer = new WindowsAbsolutePointer();
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
