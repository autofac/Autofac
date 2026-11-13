// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;

namespace Autofac.Benchmarks;

/// <summary>
/// Many tasks look up distinct services for the first time in one shared child scope, as they would
/// in a long-lived scope such as a tenant scope, so they add to its service dictionary at the same time.
/// </summary>
public class SharedChildScopeLookupBenchmark
{
    private const int ServiceCount = 256;

    private readonly IContainer _container;

    private readonly KeyedService[] _services = new KeyedService[ServiceCount];

    public SharedChildScopeLookupBenchmark()
    {
        var builder = new ContainerBuilder();
        for (var i = 0; i < ServiceCount; i++)
        {
            builder.RegisterType<Component>().Keyed<Component>(i);
            _services[i] = new KeyedService(i, typeof(Component));
        }

        _container = builder.Build();
    }

    [Params(16)]
    public int Tasks
    {
        get; set;
    }

    [Benchmark]
    public async Task FirstLookupsOnMultipleTasks()
    {
        using var scope = _container.BeginLifetimeScope(b => { });
        var registry = scope.ComponentRegistry;
        var taskCount = Tasks;
        var tasks = new Task[taskCount];

        for (var t = 0; t < taskCount; t++)
        {
            var first = t;
            tasks[t] = Task.Run(() =>
            {
                for (var i = first; i < ServiceCount; i += taskCount)
                {
                    if (!registry.TryGetServiceRegistration(_services[i], out _))
                    {
                        throw new InvalidOperationException("The service is not registered.");
                    }
                }
            });
        }

        await Task.WhenAll(tasks);
    }

    private sealed class Component
    {
    }
}
