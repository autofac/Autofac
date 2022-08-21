// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Autofac.Util.Cache;

namespace Autofac.Test.Core.Activators.Reflection;

public class MatchingSignatureConstructorSelectorTests
{
    public class FourConstructors
    {
        public FourConstructors()
        {
        }

        public FourConstructors(int i)
        {
        }

        public FourConstructors(int i, string s)
        {
        }

        public FourConstructors(int i, string s, double d)
        {
        }
    }

    [Fact]
    public void SelectsEmptyConstructor()
    {
        var target0 = new MatchingSignatureConstructorSelector();
        var c0 = target0.SelectConstructorBinding(GetConstructors(), Enumerable.Empty<Parameter>());
        Assert.Empty(c0.TargetConstructor.GetParameters());
    }

    [Fact]
    public void SelectsConstructorWithParameters()
    {
        var target2 = new MatchingSignatureConstructorSelector(typeof(int), typeof(string));
        var c2 = target2.SelectConstructorBinding(GetConstructors(), Enumerable.Empty<Parameter>());
        Assert.Equal(2, c2.TargetConstructor.GetParameters().Length);
    }

    [Fact]
    public void IgnoresInvalidBindings()
    {
        var target2 = new MatchingSignatureConstructorSelector(typeof(int), typeof(string), typeof(double));
        Assert.Throws<DependencyResolutionException>(() => target2.SelectConstructorBinding(GetConstructors(), Enumerable.Empty<Parameter>()));
    }

    [Fact]
    public void WhenNoMatchingConstructorsAvailable_ExceptionDescribesTargetTypeAndSignature()
    {
        var target = new MatchingSignatureConstructorSelector(typeof(string));

        var dx = Assert.Throws<DependencyResolutionException>(() =>
            target.SelectConstructorBinding(GetConstructors(), Enumerable.Empty<Parameter>()));

        Assert.Contains(typeof(FourConstructors).Name, dx.Message);
        Assert.Contains(typeof(string).Name, dx.Message);
    }

    [Fact]
    public void SelectsConstructorInEarlyBinding()
    {
        var target0 = new MatchingSignatureConstructorSelector();
        var c0 = target0.SelectConstructorBinder(GetConstructorBinders());
        Assert.Empty(c0.Constructor.GetParameters());
    }

    [Fact]
    public void SelectsConstructorWithParametersInEarlyBinding()
    {
        var target2 = new MatchingSignatureConstructorSelector(typeof(int), typeof(string));
        var c2 = target2.SelectConstructorBinder(GetConstructorBinders());
        Assert.Equal(2, c2.Constructor.GetParameters().Length);
    }

    [Fact]
    public void WhenNoMatchingConstructorsAvailable_ExceptionDescribesTargetTypeAndSignature_InEarlyBinding()
    {
        var target = new MatchingSignatureConstructorSelector(typeof(string));

        var dx = Assert.Throws<DependencyResolutionException>(() =>
            target.SelectConstructorBinder(GetConstructorBinders()));

        Assert.Contains(typeof(FourConstructors).Name, dx.Message);
        Assert.Contains(typeof(string).Name, dx.Message);
    }

    private static BoundConstructor[] GetConstructors()
    {
        var builder = new ContainerBuilder();
        builder.Register(ctx => 1);
        builder.Register(ctx => "test");
        var container = builder.Build();

        return GetConstructorBinders()
            .Select(cb => cb.Bind(new[] { new AutowiringParameter() }, container))
            .ToArray();
    }

    private static ConstructorBinder[] GetConstructorBinders()
    {
        return typeof(FourConstructors)
       .GetTypeInfo().DeclaredConstructors
       .Select(ci => new ConstructorBinder(ci, DefaultReflectionCache.Shared))
       .ToArray();
    }
}
