// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Test.Features.OpenGenerics;

/// <summary>
/// Regression tests for issue #1465: when one open generic component exposes several services,
/// the component it is deduplicated into must not become the default for the services that had
/// not been resolved yet, overriding registrations that should win.
/// </summary>
public class OpenGenericMultipleServiceOverrideTests
{
    private interface IFirstService
    {
    }

    private interface ISecondService
    {
    }

    private interface IFirstService<T>
    {
    }

    private interface ISecondService<T>
    {
    }

    private class NonGenericFirstOnly : IFirstService
    {
    }

    private class NonGenericSecondOnly : ISecondService
    {
    }

    private class NonGenericBothServices : IFirstService, ISecondService
    {
    }

    private class FirstOnly<T> : IFirstService<T>
    {
    }

    private class SecondOnly<T> : ISecondService<T>
    {
    }

    private class BothServices<T> : IFirstService<T>, ISecondService<T>
    {
    }

    [Fact]
    public void NonGenericOverridesWinRegardlessOfResolutionOrder()
    {
        // #1465 control case: the non-generic equivalent has always behaved correctly, and
        // the open generic behavior is expected to match it.
        using var container = BuildNonGenericContainer();

        Assert.IsType<NonGenericFirstOnly>(container.Resolve<IFirstService>());
        Assert.IsType<NonGenericSecondOnly>(container.Resolve<ISecondService>());
    }

    [Fact]
    public void OverrideWinsForServiceResolvedSecond()
    {
        // #1465: resolving IFirstService<int> pulls in BothServices<int>, which also provides
        // ISecondService<int>. That must not make BothServices<int> the default for
        // ISecondService<int> ahead of the SecondOnly<> registration that overrides it.
        using var container = BuildGenericContainer();

        Assert.IsType<FirstOnly<int>>(container.Resolve<IFirstService<int>>());
        Assert.IsType<SecondOnly<int>>(container.Resolve<ISecondService<int>>());
    }

    [Fact]
    public void OverrideWinsForServiceResolvedSecondInReverseOrder()
    {
        // #1465: the same defect in the other direction - the service resolved first is
        // irrelevant, both must get their own override.
        using var container = BuildGenericContainer();

        Assert.IsType<SecondOnly<int>>(container.Resolve<ISecondService<int>>());
        Assert.IsType<FirstOnly<int>>(container.Resolve<IFirstService<int>>());
    }

    [Fact]
    public void OverridesWinForEveryTypeArgumentAndOrderCombination()
    {
        // #1465 as reported: each closed generic gets its own service info, so the defect shows
        // up per type argument. Interleave two type arguments with opposite resolution orders.
        using var container = BuildGenericContainer();

        var firstOfInt = container.Resolve<IFirstService<int>>();
        var secondOfInt = container.Resolve<ISecondService<int>>();
        var secondOfString = container.Resolve<ISecondService<string>>();
        var firstOfString = container.Resolve<IFirstService<string>>();

        Assert.IsType<FirstOnly<int>>(firstOfInt);
        Assert.IsType<SecondOnly<int>>(secondOfInt);
        Assert.IsType<SecondOnly<string>>(secondOfString);
        Assert.IsType<FirstOnly<string>>(firstOfString);
    }

    [Fact]
    public void OverrideWinsInNestedLifetimeScope()
    {
        // #1465: service info is tracked per registry, so guard the nested scope path too.
        using var container = BuildGenericContainer();
        using var scope = container.BeginLifetimeScope();

        Assert.IsType<FirstOnly<int>>(scope.Resolve<IFirstService<int>>());
        Assert.IsType<SecondOnly<int>>(scope.Resolve<ISecondService<int>>());
    }

    [Fact]
    public void MultiServiceComponentIsDefaultWhenNothingOverridesIt()
    {
        // With no overriding registrations the multi-service component must still be the
        // default for both of its services - the fix must not suppress it.
        var builder = new ContainerBuilder();
        builder
            .RegisterGeneric(typeof(BothServices<>))
            .As(typeof(IFirstService<>))
            .As(typeof(ISecondService<>));

        using var container = builder.Build();

        Assert.IsType<BothServices<int>>(container.Resolve<IFirstService<int>>());
        Assert.IsType<BothServices<int>>(container.Resolve<ISecondService<int>>());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SharedComponentIsStillDeduplicatedAcrossServices(bool resolveFirstServiceFirst)
    {
        // The deduplication this defect came from exists so that a component exposing several
        // services resolves to one shared component rather than being queried once per service.
        // Either resolution order must yield the same instance.
        var builder = new ContainerBuilder();
        builder
            .RegisterGeneric(typeof(BothServices<>))
            .As(typeof(IFirstService<>))
            .As(typeof(ISecondService<>))
            .SingleInstance();

        using var container = builder.Build();

        object first;
        object second;

        if (resolveFirstServiceFirst)
        {
            first = container.Resolve<IFirstService<int>>();
            second = container.Resolve<ISecondService<int>>();
        }
        else
        {
            second = container.Resolve<ISecondService<int>>();
            first = container.Resolve<IFirstService<int>>();
        }

        Assert.Same(first, second);
    }

    [Fact]
    public void BothImplementationsAreAvailableFromEnumerable()
    {
        // The overridden component is still a valid registration for the service; overriding
        // only changes which one is the default.
        using var container = BuildGenericContainer();

        // Resolve the other service first to trigger the deduplication path.
        container.Resolve<IFirstService<int>>();

        var all = container.Resolve<IEnumerable<ISecondService<int>>>().ToList();

        Assert.Equal(2, all.Count);
        Assert.Contains(all, x => x is BothServices<int>);
        Assert.Contains(all, x => x is SecondOnly<int>);
    }

    private static IContainer BuildGenericContainer()
    {
        var builder = new ContainerBuilder();
        builder
            .RegisterGeneric(typeof(BothServices<>))
            .As(typeof(IFirstService<>))
            .As(typeof(ISecondService<>));
        builder
            .RegisterGeneric(typeof(FirstOnly<>))
            .As(typeof(IFirstService<>));
        builder
            .RegisterGeneric(typeof(SecondOnly<>))
            .As(typeof(ISecondService<>));

        return builder.Build();
    }

    private static IContainer BuildNonGenericContainer()
    {
        var builder = new ContainerBuilder();
        builder
            .RegisterType<NonGenericBothServices>()
            .As<IFirstService>()
            .As<ISecondService>();
        builder
            .RegisterType<NonGenericFirstOnly>()
            .As<IFirstService>();
        builder
            .RegisterType<NonGenericSecondOnly>()
            .As<ISecondService>();

        return builder.Build();
    }
}
