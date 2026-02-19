// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace Autofac.Benchmarks;

internal static class Program
{
    internal static void Main(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);
        var (filteredArgs, baselineVersion) = ExtractBaselineVersion(args);

        // Usage:
        //
        // Just run the benchmark with the source code version of the project:
        // dotnet run -c Release --project bench/Autofac.Benchmarks -- --filter *Benchmarks*
        //
        // Run the benchmark comparing the source code version to a specific package version:
        // dotnet run -c Release --project bench/Autofac.Benchmarks -- --baseline-version 9.0.0 --filter *Benchmarks*
        var config = new BenchmarkConfig();

        config.AddJob(
            Job.Default
                .WithId("Source"));

        if (!string.IsNullOrWhiteSpace(baselineVersion))
        {
            config.AddJob(
                Job.Default
                    .WithId($"Package-{baselineVersion}")
                    .AsBaseline()
                    .WithMsBuildArguments(
                        "/p:UseProjectReference=false",
                        $"/p:BaselinePackageVersion={baselineVersion}"));
        }

        new BenchmarkSwitcher(BenchmarkSet.All).Run(filteredArgs, config);
    }

    private static (string[] RemainingArgs, string? BaselineVersion) ExtractBaselineVersion(string[] args)
    {
        var forwarded = new List<string>(args.Length);
        string? baseline = null;

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (TryMatchBaselineArg(arg, out var inlineVersion))
            {
                if (!string.IsNullOrWhiteSpace(inlineVersion))
                {
                    baseline = inlineVersion;
                    continue;
                }

                if (i + 1 >= args.Length)
                {
                    throw new ArgumentException("Missing version value for baseline argument.", nameof(args));
                }

                baseline = args[++i];
                continue;
            }

            forwarded.Add(arg);
        }

        return (forwarded.ToArray(), baseline);
    }

    private static bool TryMatchBaselineArg(string arg, out string? valueFromAssignment)
    {
        valueFromAssignment = null;

        static bool Matches(string candidate) =>
            candidate.Equals("--baseline-version", StringComparison.OrdinalIgnoreCase) ||
            candidate.Equals("--baselineVersion", StringComparison.OrdinalIgnoreCase);

        var equalsIndex = arg.AsSpan().IndexOf('=');
        if (equalsIndex >= 0)
        {
            var prefix = arg[..equalsIndex];
            if (Matches(prefix))
            {
                valueFromAssignment = arg[(equalsIndex + 1)..];
                return true;
            }

            return false;
        }

        return Matches(arg);
    }
}
