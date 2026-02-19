using System.Diagnostics;
using System.Reflection;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using Autofac.Core;

namespace Autofac.BenchmarkProfiling;

/// <summary>
/// Simple command-line tool to invoke a benchmark manually in a way that helps with profiling each of the benchmarks.
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AUTOFAC_SCOPE_DIAGNOSTICS")))
        {
            AppContext.SetSwitch("Autofac.ScopeIsolatedDiagnostics", true);
        }

        // Pick a benchmark.
        var availableBenchmarks = Benchmarks.BenchmarkSet.All;

        if (args.Length == 0)
        {
            Console.WriteLine("Must provide the name of a benchmark class. (e.g. ./Autofac.BenchmarkProfiling.exe ChildScopeResolveBenchmark)");
            Console.WriteLine("Possible benchmarks are:");
            PrintBenchmarks(availableBenchmarks);
            return;
        }

        var inputType = args[0];

        var selectedBenchmark = availableBenchmarks.FirstOrDefault(x => x.Name.Equals(inputType, StringComparison.OrdinalIgnoreCase));

        if (selectedBenchmark is null)
        {
            Console.WriteLine("Specified benchmark does not exist.");
            PrintBenchmarks(availableBenchmarks);
            return;
        }

        var benchRunInfo = BenchmarkConverter.TypeToBenchmarks(selectedBenchmark);

        BenchmarkCase selectedCase = null;

        if (benchRunInfo.BenchmarksCases.Length == 0)
        {
            Console.WriteLine("No benchmark cases in specified benchmark.");
            return;
        }
        else if (benchRunInfo.BenchmarksCases.Length == 1)
        {
            selectedCase = benchRunInfo.BenchmarksCases[0];
        }
        else
        {
            // Multiple benchmark cases. Has one been supplied?
            if (args.Length > 1)
            {
                if (uint.TryParse(args[1], out var selection))
                {
                    if (selection < benchRunInfo.BenchmarksCases.Length)
                    {
                        selectedCase = benchRunInfo.BenchmarksCases[selection];
                    }
                    else
                    {
                        Console.WriteLine("Invalid benchmark case number provided. Possible options are: ");
                        PrintCases(benchRunInfo);
                    }
                }
                else
                {
                    Console.WriteLine("Cannot parse provided benchmark case selection.");
                    return;
                }
            }
            else
            {
                Console.WriteLine("Specified benchmark has multiple possible cases; a single case must be specified. Possible options are:");
                PrintCases(benchRunInfo);

                return;
            }
        }

        var benchInstance = Activator.CreateInstance(selectedCase.Descriptor.Type);

        var setupAction = BenchmarkActionFactory.CreateGlobalSetup(selectedCase.Descriptor, benchInstance);
        var cleanupAction = BenchmarkActionFactory.CreateGlobalCleanup(selectedCase.Descriptor, benchInstance);

        // Workload method is generated differently when BenchmarkDotNet actually runs; we'll need to wrap it in the set of parameters.
        // It's way slower than they way they do it, but it should still give us good profiler results.
        void workloadAction(int repeat)
        {
            while (repeat > 0)
            {
                selectedCase.Descriptor.WorkloadMethod.Invoke(benchInstance, selectedCase.Parameters.Items.Select(x => x.Value).ToArray());
                repeat--;
            }
        }

        setupAction.InvokeSingle();

        // Warmup.
        workloadAction(100);

        if (int.TryParse(Environment.GetEnvironmentVariable("AUTOFAC_MEASURE_ITERATIONS"), out var measurementIterations) &&
            measurementIterations > 0)
        {
            var sw = Stopwatch.StartNew();
            workloadAction(measurementIterations);
            sw.Stop();
            var perIteration = sw.Elapsed.TotalMilliseconds / measurementIterations;
            Console.WriteLine(
                "[Profiling] Duration: {0} iterations took {1:F2} ms (avg {2:F4} ms)",
                measurementIterations,
                sw.Elapsed.TotalMilliseconds,
                perIteration);
        }

        // Now start a new thread.
        var runThread = new Thread(new ThreadStart(() =>
        {
            // Do a lot.
            workloadAction(10000);
        }))
        {
            Name = "Workload Thread"
        };

        runThread.Start();
        runThread.Join();

        cleanupAction.InvokeSingle();

        LogScopeDiagnosticsIfEnabled();
    }

    private static void PrintBenchmarks(Type[] availableBenchmarks)
    {
        foreach (var bench in availableBenchmarks)
        {
            Console.WriteLine("  - " + bench.Name);
        }
    }

    private static void PrintCases(BenchmarkRunInfo benchRunInfo)
    {
        for (int idx = 0; idx < benchRunInfo.BenchmarksCases.Length; idx++)
        {
            var benchCase = benchRunInfo.BenchmarksCases[idx];
            if (benchCase.HasParameters)
            {
                Console.Error.WriteLine($" #{idx,-2} - {benchCase.Descriptor.DisplayInfo} ({benchCase.Parameters.DisplayInfo})");
            }
            else
            {
                Console.Error.WriteLine($" #{idx,-2} - {benchCase.Descriptor.DisplayInfo} ({benchCase.Parameters.DisplayInfo})");
            }
        }
    }

    private static void LogScopeDiagnosticsIfEnabled()
    {
        if (!AppContext.TryGetSwitch("Autofac.ScopeIsolatedDiagnostics", out var enabled) || !enabled)
        {
            return;
        }

        var diagnosticsType = typeof(IComponentRegistry).Assembly.GetType("Autofac.Core.Registration.ScopeIsolatedServiceDiagnostics");
        var snapshotProperty = diagnosticsType?.GetProperty(
            "Snapshot",
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

        if (snapshotProperty?.GetValue(null) is object snapshot)
        {
            var cacheHits = (long)(snapshot.GetType().GetProperty("CacheHits")?.GetValue(snapshot) ?? 0L);
            var cacheMisses = (long)(snapshot.GetType().GetProperty("CacheMisses")?.GetValue(snapshot) ?? 0L);
            var cacheAdds = (long)(snapshot.GetType().GetProperty("CacheAdds")?.GetValue(snapshot) ?? 0L);
            var cacheRemovals = (long)(snapshot.GetType().GetProperty("CacheRemovals")?.GetValue(snapshot) ?? 0L);
            var cachedInitializations = (long)(snapshot.GetType().GetProperty("CachedInitializations")?.GetValue(snapshot) ?? 0L);
            var discardedInfos = (long)(snapshot.GetType().GetProperty("ServiceInfoDiscarded")?.GetValue(snapshot) ?? 0L);

            Console.WriteLine(
                "[Profiling] Scope cache stats -> Hits={0}, Misses={1}, Adds={2}, Removes={3}, CachedInit={4}, Discarded={5}",
                cacheHits,
                cacheMisses,
                cacheAdds,
                cacheRemovals,
                cachedInitializations,
                discardedInfos);
        }
    }
}
