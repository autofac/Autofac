// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Benchmarks;

/// <summary>
/// Test the performance of resolving a relatively simple object graph, but within 2 levels of nested lifetime scopes,
/// to simulate a 'per-request' scope model, combined with a unit-of-work inside request.
/// </summary>
public class ConcurrencyNestedScopeBenchmark
{
    private readonly IContainer _container;

    public ConcurrencyNestedScopeBenchmark()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<MockGlobalSingleton>().SingleInstance();
        builder.RegisterType<MockRequestScopeService1>().InstancePerMatchingLifetimeScope("request");
        builder.RegisterType<MockRequestScopeService2>().InstancePerMatchingLifetimeScope("request");
        builder.RegisterType<MockUnitOfWork>().InstancePerLifetimeScope();
        _container = builder.Build();
    }

    [Params(100 /*, 100, 1_000 */)]
    public int ConcurrentRequests { get; set; }

    [Params(10)]
    public int RepeatCount { get; set; }

    [Benchmark]
    public async Task MultipleResolvesOnMultipleTasks()
    {
        var tasks = new List<Task>(ConcurrentRequests);

        for (var i = 0; i < ConcurrentRequests; i++)
        {
            var task = Task.Run(() =>
            {
                for (var j = 0; j < RepeatCount; j++)
                {
                    // Start request
                    using (var requestScope = _container.BeginLifetimeScope("request"))
                    {
                        var service1 = requestScope.Resolve<MockRequestScopeService1>();
                        if (service1 == null)
                        {
                            throw new InvalidOperationException("Service1 is null");
                        }

                        using (var unitOfWorkScope = requestScope.BeginLifetimeScope())
                        {
                            var nestedRequestService2 = unitOfWorkScope.Resolve<MockRequestScopeService2>();
                            if (nestedRequestService2 == null)
                            {
                                throw new InvalidOperationException("Nested request service is null");
                            }

                            var unitOfWork = unitOfWorkScope.Resolve<MockUnitOfWork>();
                            if (unitOfWork == null)
                            {
                                throw new InvalidOperationException("Unit of work is null");
                            }
                        }
                    }
                }
            });
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
    }

    internal class MockGlobalSingleton
    {
    }

    internal class MockRequestScopeService1
    {
        public MockRequestScopeService1()
        {
        }
    }

    internal class MockRequestScopeService2
    {
        public MockRequestScopeService2(MockRequestScopeService1 service1, MockGlobalSingleton singleton)
        {
        }
    }

    internal class MockUnitOfWork
    {
        public MockUnitOfWork(MockGlobalSingleton singleton)
        {
        }
    }
}
