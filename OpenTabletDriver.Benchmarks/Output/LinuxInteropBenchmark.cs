using System.Numerics;
using BenchmarkDotNet.Attributes;
using OpenTabletDriver.Desktop.Interop.Input.Absolute;
using OpenTabletDriver.Desktop.Interop.Input.Relative;

namespace OpenTabletDriver.Benchmarks.Output
{
    public class LinuxInteropBenchmark
    {
        private readonly EvdevAbsolutePointer absolutePointer = new EvdevAbsolutePointer();
        private readonly EvdevRelativePointer relativePointer = new EvdevRelativePointer();

        [Benchmark]
        public void EvdevAbsolute()
        {
            absolutePointer.SetPosition(Vector2.Zero);
        }

        [Benchmark]
        public void EvdevRelative()
        {
            relativePointer.SetPosition(Vector2.Zero);
        }
    }
}
