using System;
using System.IO;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;

namespace Autofac.Benchmarks
{
    internal class BenchmarkConfig : ManualConfig
    {
        private const string BenchmarkArtificatsFolder = "BenchmarkDotNet.Artifacts";

        internal BenchmarkConfig()
        {
            Add(DefaultConfig.Instance);

            var rootFolder = AppContext.BaseDirectory;
            var runFolder = DateTime.UtcNow.ToString("dd-MM-yyyy_hh-MM-ss");
            ArtifactsPath = Path.Combine(rootFolder, BenchmarkArtificatsFolder, runFolder);

            Add(MemoryDiagnoser.Default);
        }
    }
}
