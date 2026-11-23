// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Benchmarks;

/// <summary>
/// Builds a container, and begins a child scope, that each have a startable component, an
/// auto-activated component and a build callback to run.
/// </summary>
public class StartupComponentsBenchmark
{
    private readonly IContainer _container = new ContainerBuilder().Build();

    [Benchmark]
    public void BuildContainer()
    {
        var builder = new ContainerBuilder();
        Register(builder);

        using var container = builder.Build();
        GC.KeepAlive(container);
    }

    [Benchmark]
    public void BeginChildScope()
    {
        using var scope = _container.BeginLifetimeScope(Register);
        GC.KeepAlive(scope);
    }

    private static void Register(ContainerBuilder builder)
    {
        builder.RegisterType<Startable>().As<IStartable>();
        builder.RegisterType<AutoActivated>().AutoActivate();
        builder.RegisterBuildCallback(scope => { });
    }

    private sealed class Startable : IStartable
    {
        public void Start()
        {
        }
    }

    private sealed class AutoActivated
    {
    }
}
