// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;
using Autofac.Core.Lifetime;

namespace Autofac.Specification.Test.Lifetime;

public class InstancePerMatchingLifetimeScopeTests
{
    [Fact]
    public void LocalRegistration_BindingToAncestorTag_ThrowsAtScopeCreation()
    {
        // Reproduces the exact memory-leak scenario from issue #1460:
        // A child scope is created with a local registration whose matching-scope
        // tag refers to an ancestor (the outer scope). Each BeginLifetimeScope call
        // would hoist a fresh instance into the ancestor indefinitely, so we now
        // throw InvalidOperationException at scope-creation time instead.
        var builder = new ContainerBuilder();
        var container = builder.Build();

        using var outer = container.BeginLifetimeScope("outer");

        Assert.Throws<InvalidOperationException>(() =>
            outer.BeginLifetimeScope("inner", b =>
                b.RegisterType<object>().InstancePerMatchingLifetimeScope("outer")));
    }

    [Fact]
    public void LocalRegistration_BindingToScopeOwnTag_DoesNotThrow()
    {
        // A scope-local registration whose MatchingScopeLifetime tag matches the
        // NEW scope's own tag is perfectly legal: the component's lifetime exactly
        // matches its reachability, so there is no leak.
        var builder = new ContainerBuilder();
        var container = builder.Build();

        // Must not throw.
        using var scope = container.BeginLifetimeScope("x", b =>
            b.RegisterType<object>().InstancePerMatchingLifetimeScope("x"));

        // The component should be resolvable within the scope.
        var instance = scope.Resolve<object>();
        Assert.NotNull(instance);

        // And it should be shared within the same scope (singleton-within-scope semantics).
        Assert.Same(instance, scope.Resolve<object>());
    }

    [Fact]
    public void LocalRegistration_BindingToDescendantTag_DoesNotThrow()
    {
        // A scope-local registration whose MatchingScopeLifetime tag will only be
        // matched by a future descendant scope must NOT throw at registration time —
        // this is the documented, intended usage pattern.
        var builder = new ContainerBuilder();
        var container = builder.Build();

        // "child" does not yet exist in the scope chain → must not throw.
        using var parent = container.BeginLifetimeScope("parent", b =>
            b.RegisterType<object>().InstancePerMatchingLifetimeScope("child"));

        // Creating a named child scope with the matching tag should work and resolve correctly.
        using var child = parent.BeginLifetimeScope("child");
        var instance = child.Resolve<object>();
        Assert.NotNull(instance);
    }

    [Fact]
    public void LocalRegistration_BindingToRootContainerTag_ThrowsAtScopeCreation()
    {
        // The container's root tag is also a strict ancestor; binding to it from a
        // scope-local registration should likewise be rejected.
        var builder = new ContainerBuilder();
        var container = builder.Build();

        Assert.Throws<InvalidOperationException>(() =>
            container.BeginLifetimeScope("child", b =>
                b.RegisterType<object>().InstancePerMatchingLifetimeScope(LifetimeScope.RootTag)));
    }

    [Fact]
    public void LocalRegistration_BindingToAncestorTag_MultipleTagsOneAncestor_ThrowsAtScopeCreation()
    {
        // When multiple tags are supplied to InstancePerMatchingLifetimeScope and at
        // least one of them refers to an ancestor, it must still throw.
        var builder = new ContainerBuilder();
        var container = builder.Build();

        using var outer = container.BeginLifetimeScope("outer");

        Assert.Throws<InvalidOperationException>(() =>
            outer.BeginLifetimeScope("inner", b =>
                b.RegisterType<object>().InstancePerMatchingLifetimeScope("inner", "outer")));
    }

    [Fact]
    public void ChildOfNamedScopeGetsSharedInstance()
    {
        var contextName = "ctx";

        var cb = new ContainerBuilder();
        cb.RegisterType<object>().InstancePerMatchingLifetimeScope(contextName);
        var container = cb.Build();

        var ctx1 = container.BeginLifetimeScope(contextName);
        var child = ctx1.BeginLifetimeScope();

        Assert.Equal(ctx1.Resolve<object>(), child.Resolve<object>());
    }

    [Fact]
    public void InstancePerRequest_AdditionalLifetimeScopeTagsCanBeProvided()
    {
        var builder = new ContainerBuilder();
        const string Tag1 = "Tag1";
        const string Tag2 = "Tag2";
        builder.Register(c => new object()).InstancePerRequest(Tag1, Tag2);

        var container = builder.Build();

        var scope1 = container.BeginLifetimeScope(Tag1);
        Assert.NotNull(scope1.Resolve<object>());

        var scope2 = container.BeginLifetimeScope(Tag2);
        Assert.NotNull(scope2.Resolve<object>());

        var requestScope = container.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);
        Assert.NotNull(requestScope.Resolve<object>());
    }

    [Fact]
    public void InstancePerRequest_ResolutionSucceedsInRequestLifetime()
    {
        var builder = new ContainerBuilder();

        builder.RegisterType(typeof(object)).InstancePerRequest();

        var container = builder.Build();
        Assert.Throws<DependencyResolutionException>(() => container.Resolve<object>());
        Assert.Throws<DependencyResolutionException>(() => container.BeginLifetimeScope().Resolve<object>());

        var apiRequestScope = container.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);
        Assert.NotNull(apiRequestScope.Resolve<object>());
    }

    [Fact]
    public void MustHaveMatchingScope()
    {
        var cb = new ContainerBuilder();
        cb.RegisterType<object>().InstancePerMatchingLifetimeScope("ctx");
        var container = cb.Build();

        var ctx1 = container.BeginLifetimeScope();
        Assert.Throws<DependencyResolutionException>(() => ctx1.Resolve<object>());
    }

    [Fact]
    public void SharingWithinNamedScope()
    {
        var contextName = "ctx";

        var cb = new ContainerBuilder();
        cb.RegisterType<object>().InstancePerMatchingLifetimeScope(contextName);
        var container = cb.Build();

        var ctx1 = container.BeginLifetimeScope(contextName);
        var ctx2 = container.BeginLifetimeScope(contextName);

        Assert.Equal(ctx1.Resolve<object>(), ctx1.Resolve<object>());
        Assert.NotEqual(ctx1.Resolve<object>(), ctx2.Resolve<object>());
    }
}
