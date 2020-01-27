using BenchmarkDotNet.Running;

namespace Autofac.Benchmarks
{
    internal static class Program
    {
        internal static void Main(string[] args) =>
            new BenchmarkSwitcher(Benchmarks.All).Run(args, new BenchmarkConfig());
    }
}
