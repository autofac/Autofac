using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using System;
using System.Linq;
using System.Threading;

namespace Autofac.BenchmarkProfiling
{
    /// <summary>
    /// Simple command-line tool to invoke a benchmark manually in a way that helps with profiling each of the benchmarks.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // Pick a benchmark.
            var availableBenchmarks = Benchmarks.Benchmarks.All;

            if (args.Length == 0)
            {
                Console.WriteLine("Must provide the name of a benchmark class. (e.g. ./Autofac.BenchmarkProfiling.exe ChildScopeResolveBenchmark)");
                Console.WriteLine("Possible benchmarks are:");
                PrintBenchmarks(availableBenchmarks);
                return;
            }

            var inputType = args[0];

            var selectedBenchmark = availableBenchmarks.FirstOrDefault(x => x.Name.Equals(inputType, StringComparison.InvariantCultureIgnoreCase));

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
            else if(benchRunInfo.BenchmarksCases.Length == 1)
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
            Action<int> workloadAction = (repeat) =>
            {
                while (repeat > 0)
                {
                    selectedCase.Descriptor.WorkloadMethod.Invoke(benchInstance, selectedCase.Parameters.Items.Select(x => x.Value).ToArray());
                    repeat--;
                }
            };

            setupAction.InvokeSingle();

            // Warmup.
            workloadAction(100);

            // Now start a new thread.
            var runThread = new Thread(new ThreadStart(() =>
            {
                // Do a lot.
                workloadAction(10000);
            }));

            runThread.Name = "Workload Thread";

            runThread.Start();
            runThread.Join();

            cleanupAction.InvokeSingle();
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
    }
}
