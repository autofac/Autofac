// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Benchmarks;

/// <summary>
/// Measures container build (registration) time, isolated from any resolve activity.
/// </summary>
/// <remarks>
/// Building a container constructs a
/// <see cref="Autofac.Core.Activators.Reflection.ReflectionActivator"/> for each
/// reflection-based registration, which inspects the implementation type's members.
/// This benchmark guards that per-registration cost as the registration count grows.
/// </remarks>
public class ContainerBuildBenchmark
{
    [Params(1000, 5000)]
    public int RegistrationCount
    {
        get; set;
    }

    [Benchmark]
    public void Build()
    {
        var builder = new ContainerBuilder();

        for (var i = 0; i < RegistrationCount; i++)
        {
            builder.RegisterType<Component>().As<IComponent>();
        }

        using var container = builder.Build();
        GC.KeepAlive(container);
    }

    private interface IComponent
    {
    }

    private sealed class Component : IComponent
    {
    }
}
