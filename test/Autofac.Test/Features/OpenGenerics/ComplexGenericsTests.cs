// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Core.Registration;

namespace Autofac.Test.Features.OpenGenerics;

public class ComplexGenericsTests
{
    private interface I1<T>
    {
    }

    private interface I2<T>
    {
    }

    private interface IConstrainedConstraint<T>
        where T : IEquatable<int>
    {
    }

    private interface IConstrainedConstraintWithAddedArgument<T1, T2> : IConstrainedConstraint<T2>
        where T2 : IEquatable<int>
    {
    }

    private interface IConstrainedConstraintWithOnlyAddedArgument<T1> : IConstrainedConstraintWithAddedArgument<T1, int>
    {
    }

    private interface IConstraint<T>
    {
    }

    private interface IConstraintWithAddedArgument<T2, T1> : IConstraint<T1>
    {
    }

    private interface IDouble<T2, T3>
    {
    }

    private interface IDoubleGenericWithInModifier<in T1, T2>
    {
    }

    private interface INested<T>
    {
    }

    private interface INested<T, TD>
    {
    }

    private interface IOtherSimpleInterface
    {
    }

    private interface ISimpleInterface
    {
    }

    private interface ISingle<T> : IDouble<T, int>
    {
    }

    private interface ISingleGeneric<T>
    {
    }

    private interface ISingleGenericWithInModifier<in T>
    {
    }

    private interface ISingleGenericWithOutModifier<out T>
    {
    }

    [Fact]
    public void CanResolveByGenericInterface()
    {
        var builder = new ContainerBuilder();

        builder.RegisterGeneric(typeof(CompanyA.CompositeValidator<>)).As(typeof(CompanyA.IValidator<>));

        var container = builder.Build();

        var validator = container.Resolve<CompanyA.IValidator<int>>();
        Assert.IsType<CompanyA.CompositeValidator<int>>(validator);
    }

    [Fact]
    public void CanResolveComponentUsingInterfaceWhenConstraintAndClassUseNestedInterfaces()
    {
        var builder = new ContainerBuilder();

        builder.RegisterGeneric(typeof(CNestedConstrained<,>))
            .As(typeof(IDoubleGenericWithInModifier<,>));

        builder.RegisterGeneric(typeof(COtherNestedConstrained<,>))
            .As(typeof(IDoubleGenericWithInModifier<,>));

        var container = builder.Build();

        // These simple concrete implementations lookup which were failing as per issue #794
        Assert.True(container.IsRegistered<IDoubleGenericWithInModifier<int, ISingleGenericWithOutModifier<CSimple>>>());
        Assert.True(container.IsRegistered<IDoubleGenericWithInModifier<int, ISingleGenericWithOutModifier<COther>>>());

        // These a little bit more complex concrete implementations lookup were failing as well as per issue #794
        Assert.True(container.IsRegistered<IDoubleGenericWithInModifier<ISingleGeneric<ISingleGenericWithOutModifier<CSimple>>, ISingleGenericWithOutModifier<CSimple>>>());
        Assert.True(container.IsRegistered<IDoubleGenericWithInModifier<ISingleGeneric<ISingleGenericWithOutModifier<COther>>, ISingleGenericWithOutModifier<COther>>>());

        // These should resolve, but per issue #794 exceptions were thrown
        container.Resolve<IDoubleGenericWithInModifier<ISingleGeneric<ISingleGenericWithOutModifier<CSimple>>, ISingleGenericWithOutModifier<CSimple>>>();
        container.Resolve<IDoubleGenericWithInModifier<ISingleGeneric<ISingleGenericWithOutModifier<CSimple>>, ISingleGenericWithOutModifier<COther>>>();
    }

    [Fact]
    public void CanResolveComponentWhenConstrainedArgumentIsGenericTypeWithMoreArgumentsThanGenericConstraint()
    {
        var builder = new ContainerBuilder();

        builder.RegisterGeneric(typeof(Constrained<,>));

        var container = builder.Build();

        Assert.True(container.IsRegistered<Constrained<int, IConstraintWithAddedArgument<string, int>>>());
    }

    [Fact]
    public void CanResolveComponentWhenConstraintsAreNested()
    {
        var builder = new ContainerBuilder();

        builder.RegisterGeneric(typeof(MultiConstrained<,>));

        var container = builder.Build();

        Assert.True(container.IsRegistered<MultiConstrained<int, IConstrainedConstraintWithOnlyAddedArgument<string>>>());
    }

    [Fact]
    public void CanResolveComponentWhenGenericParameterIsConstrainedWithOtherGenericParameter()
    {
        var builder = new ContainerBuilder();

        builder.RegisterGeneric(typeof(ConstrainedWithGenericParameter<,>));

        var container = builder.Build();

        Assert.True(container.IsRegistered<ConstrainedWithGenericParameter<int, object>>());
    }

    [Fact]
    public void CanResolveComponentWhenItInheritsFromTheSameGenericInterfaceTwice()
    {
        var builder = new ContainerBuilder();

        builder.RegisterGeneric(typeof(CDoubleInheritGeneric<>)).As(typeof(ISingleGenericWithInModifier<>));

        var container = builder.Build();

        // These should resolve because it inherits from ISingleGenericWithInModifier with both as nested generic arguments
        Assert.True(container.IsRegistered<ISingleGenericWithInModifier<ISingleGeneric<int>>>());
        Assert.True(container.IsRegistered<ISingleGenericWithInModifier<CGeneric<int>>>());
        Assert.True(container.IsRegistered<ISingleGenericWithInModifier<ISingleGeneric<CSimple>>>());
        Assert.True(container.IsRegistered<ISingleGenericWithInModifier<CGeneric<CSimple>>>());

        // These should not resolve because they do not match the above pattern
        Assert.False(container.IsRegistered<ISingleGenericWithInModifier<int>>());
        Assert.False(container.IsRegistered<ISingleGenericWithInModifier<ISingleGenericWithOutModifier<int>>>());
        Assert.False(container.IsRegistered<ISingleGenericWithInModifier<ISingleGenericWithOutModifier<CSimple>>>());
    }

    [Fact]
    public void CanResolveComponentWithNestedConstraintViaInterface()
    {
        var builder = new ContainerBuilder();

        builder.RegisterGeneric(typeof(Constrained<,>));

        var container = builder.Build();

        Assert.True(container.IsRegistered<Constrained<int, IConstraint<int>>>());
    }

    [Fact]
    public void CanResolveComponentWithNestedEnumerableConstraint()
    {
        // Issue #972
        var builder = new ContainerBuilder();
        builder.RegisterGeneric(typeof(CollectionConstrainedByClass<,>));
        var container = builder.Build();
        Assert.True(typeof(ConstraintClass<IEnumerable<string>>).IsAssignableFrom(typeof(CollectionOfStrings)));
        Assert.IsType<CollectionConstrainedByClass<string, CollectionOfStrings>>(container.Resolve<CollectionConstrainedByClass<string, CollectionOfStrings>>());
    }

    [Fact]
    public void CanResolveConcreteTypesThatReorderImplementedInterfaceParameters()
    {
        var cb = new ContainerBuilder();
        cb.RegisterGeneric(typeof(CReversed<,>));
        var container = cb.Build();

        var self = container.Resolve<CReversed<int, string>>();
        Assert.IsType<CReversed<int, string>>(self);
    }

    [Fact]
    public void CanResolveImplementationsWhereTypeParametersAreReordered()
    {
        var cb = new ContainerBuilder();
        cb.RegisterGeneric(typeof(CReversed<,>)).As(typeof(IDouble<,>));
        var container = cb.Build();

        var repl = container.Resolve<IDouble<int, string>>();
        Assert.IsType<CReversed<string, int>>(repl);
    }

    [Fact]
    public void GenericArgumentArityDifference()
    {
        // Issue #688
        var builder = new ContainerBuilder();
        builder.RegisterGeneric(typeof(CDerivedSingle<>)).AsImplementedInterfaces();
        var container = builder.Build();
        Assert.IsType<CDerivedSingle<double>>(container.Resolve<ISingle<double>>());
        Assert.IsType<CDerivedSingle<double>>(container.Resolve<IDouble<double, int>>());
    }

    [Fact]
    public void MultipleServicesOnAnOpenGenericType_ShareTheSameRegistration()
    {
        var builder = new ContainerBuilder();
        builder.RegisterGeneric(typeof(C<>)).As(typeof(I1<>), typeof(I2<>));
        var container = builder.Build();
        container.Resolve<I1<int>>();
        var count = container.ComponentRegistry.Registrations.Count();
        container.Resolve<I2<int>>();
        Assert.Equal(count, container.ComponentRegistry.Registrations.Count());
    }

    [Fact]
    public void NestedGenericClassesCanBeResolved()
    {
        var cb = new ContainerBuilder();
        cb.RegisterGeneric(typeof(CNestedDerived<>)).As(typeof(CNested<>));
        var container = cb.Build();

        var nest = container.Resolve<CNested<Wrapper<string>>>();
        Assert.IsType<CNestedDerived<Wrapper<string>>>(nest);
    }

    [Fact]
    public void NestedGenericInterfacesCanBeResolved()
    {
        var cb = new ContainerBuilder();
        cb.RegisterGeneric(typeof(CNested<>)).As(typeof(INested<>));
        var container = cb.Build();

        var nest = container.Resolve<INested<Wrapper<string>>>();
        Assert.IsType<CNested<string>>(nest);
    }

    [Fact]
    public void TestNestingAndReversingSimplification()
    {
        var cb = new ContainerBuilder();
        cb.RegisterGeneric(typeof(CNestedDerivedReversed<,>)).As(typeof(IDouble<,>));
        var container = cb.Build();

        var compl = container.Resolve<IDouble<int, INested<Wrapper<string>>>>();
        Assert.IsType<CNestedDerivedReversed<string, int>>(compl);
    }

    [Fact]
    public void TestReversingWithoutNesting()
    {
        var cb = new ContainerBuilder();
        cb.RegisterGeneric(typeof(CReversed<,>)).As(typeof(IDouble<,>));
        var container = cb.Build();

        var compl = container.Resolve<IDouble<int, INested<Wrapper<string>>>>();
        Assert.IsType<CReversed<INested<Wrapper<string>>, int>>(compl);
    }

    [Fact]
    public void TheSamePlaceholderTypeCanAppearMultipleTimesInTheService()
    {
        var cb = new ContainerBuilder();
        cb.RegisterGeneric(typeof(SameTypes<,>)).As(typeof(SameTypes<,>).GetInterfaces());
        var container = cb.Build();

        var compl = container.Resolve<IDouble<int, INested<IDouble<string, int>>>>();
        Assert.IsType<SameTypes<int, string>>(compl);
    }

    [Fact]
    public void TheSamePlaceholderWithThreeGenericParametersTypeCanAppearMultipleTimesInTheService()
    {
        var cb = new ContainerBuilder();
        cb.RegisterGeneric(typeof(SameTypes<,,>)).As(typeof(SameTypes<,,>).GetTypeInfo().ImplementedInterfaces.ToArray());
        var container = cb.Build();

        var compl = container.Resolve<IDouble<INested<string>, INested<IDouble<int, long>>>>();
        Assert.IsType<SameTypes<string, int, long>>(compl);
    }

    [Fact]
    public void WhenTheSameTypeAppearsMultipleTimesInTheImplementationMappingItMustAlsoInTheService()
    {
        var cb = new ContainerBuilder();
        cb.RegisterGeneric(typeof(SameTypes<,>)).As(typeof(SameTypes<,>).GetInterfaces());
        var container = cb.Build();

        Assert.Throws<ComponentNotRegisteredException>(() =>
            container.Resolve<IDouble<decimal, INested<IDouble<string, int>>>>());
    }

    [Fact]
    public void TestSelfReferentialGeneric()
    {
        var cb = new ContainerBuilder();
        cb.RegisterGeneric(typeof(SelfReferenceConsumer<>)).As(typeof(IBaseGeneric<>));
        var container = cb.Build();
        container.Resolve<IBaseGeneric<DerivedSelfReferencing>>();
    }

    [Fact]
    public void ResolveTypeFromOpenGenericInterfaceTypeParameterIsInterfaceWithConstraint()
    {
        var cb = new ContainerBuilder();
        cb.RegisterGeneric(typeof(CGenericNestedProvider<>)).AsImplementedInterfaces();
        cb.RegisterType<CNestedSimpleInterface>().AsImplementedInterfaces();
        var container = cb.Build();
        container.Resolve<INested<ISimpleInterface>>();
    }

    [Fact]
    public void CheckGenericTypeIsRegisteredWhenNotSpecifyParameterType()
    {
        // Issue #1040: IsRegistered throws IndexOutOfRangeException for open generic type
        var cb = new ContainerBuilder();
        cb.RegisterGeneric(typeof(C<>));
        var container = cb.Build();

        Assert.False(container.IsRegistered(typeof(C<>)));
    }

    private class CNestedSimpleInterface : INested<ISimpleInterface>
    {
    }

    private class CGenericNestedProvider<T> : INested<T>
        where T : CSimpleNoInterface
    {
    }

    private class CSimpleNoInterface
    {
    }

    private class C<T> : I1<T>, I2<T>
    {
    }

    private class CDerivedSingle<T> : ISingle<T>
    {
    }

    private class CDoubleInheritGeneric<T> : ISingleGenericWithInModifier<ISingleGeneric<T>>, ISingleGenericWithInModifier<CGeneric<T>>
    {
    }

    private class CGeneric<T> : ISingleGeneric<T>
    {
    }

    private class CNested<T> : INested<Wrapper<T>>
    {
    }

    private class CNestedConstrained<T1, T2> : IDoubleGenericWithInModifier<T1, T2>
        where T2 : ISingleGenericWithOutModifier<ISimpleInterface>
    {
    }

    private class CNestedDerived<T> : CNested<T>
    {
    }

    private class CNestedDerivedReversed<TX, TY> : IDouble<TY, INested<Wrapper<TX>>>
    {
    }

    private class CollectionConstrainedByClass<TInput, TCollection>
        where TCollection : ConstraintClass<IEnumerable<TInput>>
    {
    }

    private class CollectionOfStrings : ConstraintClass<IEnumerable<string>>
    {
    }

    private class Constrained<T1, T2>
        where T2 : IConstraint<T1>
    {
    }

    private class ConstrainedWithGenericParameter<T1, T2>
        where T1 : T2
    {
    }

    private class ConstraintClass<T>
    {
    }

    private class COther : IOtherSimpleInterface
    {
    }

    private class COtherNestedConstrained<T1, T2> : IDoubleGenericWithInModifier<T1, T2>
        where T2 : ISingleGenericWithOutModifier<IOtherSimpleInterface>
    {
    }

    private class CReversed<T2, T1> : IDouble<T1, T2>
    {
    }

    private class CSimple : ISimpleInterface
    {
    }

    private class MultiConstrained<T1, T2>
        where T1 : IEquatable<int>
        where T2 : IConstrainedConstraint<T1>
    {
    }

    private class SameTypes<TA, TB> : IDouble<TA, INested<IDouble<TB, TA>>>
    {
    }

    private class SameTypes<TA, TB, TC> : IDouble<INested<TA>, INested<IDouble<TB, TC>>>
    {
    }

    private class Wrapper<T>
    {
    }

    private interface IBaseGeneric<TDerived>
        where TDerived : BaseGenericImplementation<TDerived>, new()
    {
    }

    private class SelfReferenceConsumer<TSource> : IBaseGeneric<TSource>
        where TSource : BaseGenericImplementation<TSource>, new()
    {
    }

    private abstract class BaseGenericImplementation<TDerived>
    {
    }

    private class DerivedSelfReferencing : BaseGenericImplementation<DerivedSelfReferencing>
    {
    }
}
