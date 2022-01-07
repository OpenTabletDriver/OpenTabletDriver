using BenchmarkDotNet.Running;

namespace OpenTabletDriver.Benchmarks
{
    public class Benchmark
    {
        public static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(typeof(Benchmark).Assembly).Run(args);
        }
    }
}
