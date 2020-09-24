using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Autofac.Benchmarks
{
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
                    try
                    {
                        for (var j = 0; j < RepeatCount; j++)
                        {
                            // Start request
                            using(var requestScope = _container.BeginLifetimeScope("request"))
                            {
                                var service1 = requestScope.Resolve<MockRequestScopeService1>();
                                Assert.NotNull(service1);

                                using (var unitOfWorkScope = requestScope.BeginLifetimeScope())
                                {
                                    var nestedRequestService2 = unitOfWorkScope.Resolve<MockRequestScopeService2>();
                                    Assert.NotNull(nestedRequestService2);
                                    var unitOfWork = unitOfWorkScope.Resolve<MockUnitOfWork>();
                                    Assert.NotNull(unitOfWork);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Assert.True(false, ex.ToString());
                    }
                });
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        // Disable "unused parameter" warnings for test types.
#pragma warning disable IDE0060

        internal class MockGlobalSingleton
        {
        }

        internal class MockRequestScopeService1
        {
            public MockRequestScopeService1() { }
        }

        internal class MockRequestScopeService2
        {
            public MockRequestScopeService2(MockRequestScopeService1 service1, MockGlobalSingleton singleton) { }
        }

        internal class MockUnitOfWork
        {
            public MockUnitOfWork(MockGlobalSingleton singleton) {}
        }

#pragma warning restore IDE0060
    }
}
