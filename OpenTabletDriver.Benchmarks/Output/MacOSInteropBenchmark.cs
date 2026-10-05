using System.Numerics;
using BenchmarkDotNet.Attributes;
using NSubstitute;
using OpenTabletDriver.Desktop.Interop.Input.Absolute;
using OpenTabletDriver.Desktop.Interop.Input.Relative;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Platform.Keyboard;

namespace OpenTabletDriver.Benchmarks.Output
{
    public class MacOSInteropBenchmark
    {
        private MacOSAbsolutePointer absolutePointer = new MacOSAbsolutePointer(Substitute.For<IVirtualScreen>()!, Substitute.For<IVirtualKeyboard>()!);
        private MacOSRelativePointer relativePointer = new MacOSRelativePointer(Substitute.For<IVirtualKeyboard>()!);

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
