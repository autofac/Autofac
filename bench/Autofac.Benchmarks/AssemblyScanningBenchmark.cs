// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Benchmarks;

/// <summary>
/// Measures building a container from assembly scanning, where each scanned type is registered
/// as the interfaces it implements.
/// </summary>
public class AssemblyScanningBenchmark
{
    [Benchmark]
    public void BuildWithImplementedInterfaces()
    {
        var builder = new ContainerBuilder();
        builder.RegisterAssemblyTypes(typeof(AssemblyScanningBenchmark).Assembly)
            .Where(t => t.DeclaringType == typeof(AssemblyScanningBenchmark))
            .AsImplementedInterfaces();

        using var container = builder.Build();
        GC.KeepAlive(container);
    }

    private interface IFirst
    {
    }

    private interface ISecond
    {
    }

    private interface IThird
    {
    }

    private sealed class Scanned1 : IFirst, ISecond, IThird
    {
    }

    private sealed class Scanned2 : IFirst, ISecond, IThird
    {
    }

    private sealed class Scanned3 : IFirst, ISecond, IThird
    {
    }

    private sealed class Scanned4 : IFirst, ISecond, IThird
    {
    }

    private sealed class Scanned5 : IFirst, ISecond, IThird
    {
    }

    private sealed class Scanned6 : IFirst, ISecond, IThird
    {
    }

    private sealed class Scanned7 : IFirst, ISecond, IThird
    {
    }

    private sealed class Scanned8 : IFirst, ISecond, IThird
    {
    }

    private sealed class Scanned9 : IFirst, ISecond, IThird
    {
    }

    private sealed class Scanned10 : IFirst, ISecond, IThird
    {
    }

    private sealed class Scanned11 : IFirst, ISecond, IThird
    {
    }

    private sealed class Scanned12 : IFirst, ISecond, IThird
    {
    }
}
