using System;
using System.IO;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;

namespace Autofac.Benchmarks
{
    internal class BenchmarkConfig : ManualConfig
    {
        private const string BenchmarkArtifactsFolder = "BenchmarkDotNet.Artifacts";

        internal BenchmarkConfig()
        {
            Add(DefaultConfig.Instance);

            var rootFolder = AppContext.BaseDirectory;
            var runFolder = DateTime.UtcNow.ToString("dd-MM-yyyy_hh-MM-ss");
            ArtifactsPath = Path.Combine(rootFolder, BenchmarkArtifactsFolder, runFolder);

            AddDiagnoser(MemoryDiagnoser.Default);
        }
    }
}
