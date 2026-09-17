// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Test.Concurrency;

/// <summary>
/// Concurrency guard for issue #1465. One open generic component exposing several services holds its
/// implementation against the source that produced it until the other services drain that source,
/// and two of those services can be first resolved on different threads at once.
/// </summary>
public sealed class MultiServiceOpenGenericConcurrencyTests
{
    private interface IFirstService<T>
    {
    }

    private interface ISecondService<T>
    {
    }

    private class BothServices<T> : IFirstService<T>, ISecondService<T>
    {
    }

    [Fact]
    public async Task ConcurrentFirstResolveOfBothServicesDoesNotThrowOrDeadlock()
    {
        // Two threads resolving two services of one component each hold the service info monitor the
        // other would need, so holding an implementation must never block on the other service's
        // info, and must never rebuild a queue another thread is enumerating.
        for (var i = 0; i < 50; i++)
        {
            var builder = new ContainerBuilder();
            builder
                .RegisterGeneric(typeof(BothServices<>))
                .As(typeof(IFirstService<>))
                .As(typeof(ISecondService<>));

            using var container = builder.Build();

            var first = Task.Run(() => (object)container.Resolve<IFirstService<int>>());
            var second = Task.Run(() => (object)container.Resolve<ISecondService<int>>());

            var resolved = await Task.WhenAll(first, second).WaitAsync(TimeSpan.FromSeconds(30));

            Assert.All(resolved, Assert.NotNull);
        }
    }
}
