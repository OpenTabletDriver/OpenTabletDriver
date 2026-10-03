using System.Numerics;
using BenchmarkDotNet.Attributes;
using NSubstitute;
using OpenTabletDriver.Desktop.Interop.Input.Absolute;
using OpenTabletDriver.Desktop.Interop.Input.Relative;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Benchmarks.Output
{
    public class LinuxInteropBenchmark
    {
        EvdevAbsolutePointer absolutePointer = new EvdevAbsolutePointer(Substitute.For<IVirtualScreen>()!, Substitute.For<TabletReference>()!);
        EvdevRelativePointer relativePointer = new EvdevRelativePointer(Substitute.For<TabletReference>()!);

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
