using System.Numerics;
using BenchmarkDotNet.Attributes;
using OpenTabletDriver.Desktop.Interop.Input.Absolute;
using OpenTabletDriver.Desktop.Interop.Input.Relative;

namespace OpenTabletDriver.Benchmarks.Output
{
    public class LinuxInteropBenchmark
    {
        EvdevAbsolutePointer absolutePointer = new EvdevAbsolutePointer();
        EvdevRelativePointer relativePointer = new EvdevRelativePointer();

        [Benchmark]
        public void EvdevAbsolute()
        {
            absolutePointer.HandlePoint(Vector2.Zero);
        }

        [Benchmark]
        public void EvdevRelative()
        {
            relativePointer.HandlePoint(Vector2.Zero);
        }
    }
}