// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Test.Features.OpenGenerics;

/// <summary>
/// Regression tests for issue #1464: OpenGenericServiceBinder.TryBindOpenGenericTypedService
/// should not let a non-mappable interface appearing first in the interface list suppress a
/// valid later interface that CAN be mapped.
/// </summary>
public class OpenGenericMultipleInterfaceOrderTests
{
    private interface IHandler<in T>
    {
    }

    private interface IRequest
    {
    }

    private interface IRequest<T>
    {
    }

    /// <summary>
    /// Handler whose first interface IS the mappable one (IHandler&lt;IRequest&lt;TParam&gt;&gt;).
    /// This worked before the fix and must continue to work.
    /// </summary>
    private class HandlerMappableFirst<TParam>
        : IHandler<IRequest<TParam>>, IHandler<IRequest>
    {
    }

    /// <summary>
    /// Handler whose first interface is NOT mappable for the requested service
    /// (IHandler&lt;IRequest&gt; — closed, no TParam) and the mappable one comes second.
    /// This is the ordering bug reported in #1464.
    /// </summary>
    private class HandlerNonMappableFirst<TParam>
        : IHandler<IRequest>, IHandler<IRequest<TParam>>
    {
    }

    /// <summary>
    /// Handler whose class generic parameter is NOT used in the interface it implements.
    /// Assignability rules say this CANNOT be bound as an open-generic IHandler&lt;&gt; because
    /// TParam cannot be inferred from the service arguments. Autofac must not bind this
    /// and must not throw; it must cleanly return no registration.
    /// </summary>
    private class HandlerWithUnrelatedTypeParam<TParam>
        : IHandler<int>
    {
    }

    [Fact]
    public void MappableFirstHandlerIsResolvable()
    {
        var builder = new ContainerBuilder();
        builder
            .RegisterGeneric(typeof(HandlerMappableFirst<>))
            .As(typeof(IHandler<>).MakeGenericType(typeof(IRequest<>)));

        var container = builder.Build();

        var handlers = container.Resolve<IEnumerable<IHandler<IRequest<int>>>>();
        Assert.Single(handlers);
        Assert.IsType<HandlerMappableFirst<int>>(handlers.Single());
    }

    [Fact]
    public void NonMappableFirstHandlerIsResolvable()
    {
        // Regression for #1464: when the first interface (IHandler<IRequest>) cannot
        // be mapped to the type parameter TParam, the binder must not give up — it
        // must continue to the second interface (IHandler<IRequest<TParam>>) which CAN
        // be mapped.
        var builder = new ContainerBuilder();
        builder
            .RegisterGeneric(typeof(HandlerNonMappableFirst<>))
            .As(typeof(IHandler<>).MakeGenericType(typeof(IRequest<>)));

        var container = builder.Build();

        var handlers = container.Resolve<IEnumerable<IHandler<IRequest<int>>>>();
        Assert.Single(handlers);
        Assert.IsType<HandlerNonMappableFirst<int>>(handlers.Single());
    }

    [Fact]
    public void BothHandlerOrderVariantsAreResolvedFromEnumerable()
    {
        // The full scenario from the issue: both handlers registered for the same open
        // generic service — both must appear in the resolved enumerable regardless of
        // which one has the non-mappable interface first.
        var builder = new ContainerBuilder();
        builder
            .RegisterGeneric(typeof(HandlerMappableFirst<>))
            .As(typeof(IHandler<>).MakeGenericType(typeof(IRequest<>)));
        builder
            .RegisterGeneric(typeof(HandlerNonMappableFirst<>))
            .As(typeof(IHandler<>).MakeGenericType(typeof(IRequest<>)));

        var container = builder.Build();

        var handlers = container.Resolve<IEnumerable<IHandler<IRequest<int>>>>().ToList();
        Assert.Equal(2, handlers.Count);
        Assert.Contains(handlers, h => h is HandlerMappableFirst<int>);
        Assert.Contains(handlers, h => h is HandlerNonMappableFirst<int>);
    }

    [Fact]
    public void HandlerWithUnrelatedTypeParamIsNotResolved()
    {
        // Second case from #1464: GenericMismatchWithInterface<TParam> : IHandler<int>
        // registered as IHandler<>. The type parameter TParam cannot be inferred from the
        // service arguments (int), so Autofac must not bind this and the enumerable must
        // be empty. No exception should be thrown.
        var builder = new ContainerBuilder();
        builder
            .RegisterGeneric(typeof(HandlerWithUnrelatedTypeParam<>))
            .As(typeof(IHandler<>));

        var container = builder.Build();

        // Must not throw; must return no registrations.
        var handlers = container.Resolve<IEnumerable<IHandler<int>>>();
        Assert.Empty(handlers);
    }
}
