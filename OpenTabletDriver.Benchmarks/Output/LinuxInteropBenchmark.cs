using System.Numerics;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Daemon.Interop;
using OpenTabletDriver.Platform.Pointer;

namespace OpenTabletDriver.Benchmarks.Output
{
    public class LinuxInteropBenchmark
    {
        private readonly IAbsolutePointer _absolutePointer;
        private readonly IRelativePointer _relativePointer;

        public LinuxInteropBenchmark()
        {
            var serviceProvider = new DesktopLinuxServiceCollection().BuildServiceProvider();
            _absolutePointer = serviceProvider.GetRequiredService<IAbsolutePointer>();
            _relativePointer = serviceProvider.GetRequiredService<IRelativePointer>();
        }

        [Benchmark]
        public void EvdevAbsolute()
        {
            _absolutePointer.SetPosition(Vector2.Zero);
        }

        [Benchmark]
        public void EvdevRelative()
        {
            _relativePointer.SetPosition(Vector2.Zero);
        }
    }
}
