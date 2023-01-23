using System.Numerics;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Daemon.Interop;
using OpenTabletDriver.Platform.Pointer;

namespace OpenTabletDriver.Benchmarks.Output
{
    public class MacOSInteropBenchmark
    {
        private readonly IAbsolutePointer _absolutePointer;
        private readonly IRelativePointer _relativePointer;

        public MacOSInteropBenchmark()
        {
            var serviceProvider = new DesktopMacOSServiceCollection().BuildServiceProvider();
            _absolutePointer = serviceProvider.GetRequiredService<IAbsolutePointer>();
            _relativePointer = serviceProvider.GetRequiredService<IRelativePointer>();
        }

        [Benchmark]
        public void CoreGraphicsAbsolute()
        {
            _absolutePointer.SetPosition(Vector2.Zero);
        }

        [Benchmark]
        public void CoreGraphicsRelative()
        {
            _relativePointer.SetPosition(Vector2.Zero);
        }
    }
}
