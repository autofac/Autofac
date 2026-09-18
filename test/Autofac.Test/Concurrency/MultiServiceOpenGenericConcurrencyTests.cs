// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;

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
    public async Task ConcurrentFirstResolveOfBothServicesDoesNotDeadlock()
    {
        // Issue #1465: two threads resolving two services of one component each hold the service
        // info monitor the other would need, so holding an implementation must never block on the
        // other service's info, and must never rebuild a queue another thread is enumerating.
        // Losing this race can still fail the resolve - see issue #1500 - so only completion is
        // asserted here.
        for (var i = 0; i < 50; i++)
        {
            var builder = new ContainerBuilder();
            builder
                .RegisterGeneric(typeof(BothServices<>))
                .As(typeof(IFirstService<>))
                .As(typeof(ISecondService<>));

            using var container = builder.Build();

            var first = Task.Run(() => ResolveIgnoringLostRace(() => container.Resolve<IFirstService<int>>()));
            var second = Task.Run(() => ResolveIgnoringLostRace(() => container.Resolve<ISecondService<int>>()));
            var completion = Task.WhenAll(first, second);

            await completion.WaitAsync(TimeSpan.FromSeconds(30));

            Assert.True(completion.IsCompletedSuccessfully);
        }
    }

    private static void ResolveIgnoringLostRace(Func<object> resolve)
    {
        try
        {
            resolve();
        }
        catch (DependencyResolutionException)
        {
            // Issue #1500: the two threads can each query the source, which fails the resolve. A
            // deadlock would instead hang, which is what the caller's timeout catches.
        }
    }
}
