using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Core.Registration;
using Xunit;

namespace Autofac.Test.Features.OpenGenerics
{
    public class ComplexGenericsTests
    {
        public interface I1<T>
        {
        }

        public interface I2<T>
        {
        }

        public interface IConstrainedConstraint<T>
            where T : IEquatable<int>
        {
        }

        public interface IConstrainedConstraintWithAddedArgument<T1, T2> : IConstrainedConstraint<T2>
            where T2 : IEquatable<int>
        {
        }

        public interface IConstrainedConstraintWithOnlyAddedArgument<T1> : IConstrainedConstraintWithAddedArgument<T1, int>
        {
        }

        public interface IConstraint<T>
        {
        }

        public interface IConstraintWithAddedArgument<T2, T1> : IConstraint<T1>
        {
        }

        public interface IDouble<T2, T3>
        {
        }

        public interface IDoubleGenericWithInModifier<in T1, T2>
        {
        }

        public interface INested<T>
        {
        }

        public interface INested<T, TD>
        {
        }

        public interface IOtherSimpleInterface
        {
        }

        public interface ISimpleInterface
        {
        }

        public interface ISingle<T> : IDouble<T, int>
        {
        }

        public interface ISingleGeneric<T>
        {
        }

        public interface ISingleGenericWithInModifier<in T>
        {
        }

        public interface ISingleGenericWithOutModifier<out T>
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

            builder.RegisterGeneric(typeof(CNestedConstrainted<,>))
                .As(typeof(IDoubleGenericWithInModifier<,>));

            builder.RegisterGeneric(typeof(COtherNestedConstrainted<,>))
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

            var instance = container.Resolve<IBaseGeneric<DerivedSelfReferencing>>();
        }

        [Fact]
        public void ResolveTypeFromOpenGenericInterfaceTypeParameterIsInterfaceWithConstraint()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(CGenericNestedProvider<>)).AsImplementedInterfaces();
            cb.RegisterType<CNestedSimpleInterface>().AsImplementedInterfaces();
            var container = cb.Build();

            var instance = container.Resolve<INested<ISimpleInterface>>();
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

        public class C<T> : I1<T>, I2<T>
        {
        }

        public class CDerivedSingle<T> : ISingle<T>
        {
        }

        public class CDoubleInheritGeneric<T> : ISingleGenericWithInModifier<ISingleGeneric<T>>, ISingleGenericWithInModifier<CGeneric<T>>
        {
        }

        public class CGeneric<T> : ISingleGeneric<T>
        {
        }

        public class CNested<T> : INested<Wrapper<T>>
        {
        }

        public class CNestedConstrainted<T1, T2> : IDoubleGenericWithInModifier<T1, T2>
            where T2 : ISingleGenericWithOutModifier<ISimpleInterface>
        {
        }

        public class CNestedDerived<T> : CNested<T>
        {
        }

        public class CNestedDerivedReversed<TX, TY> : IDouble<TY, INested<Wrapper<TX>>>
        {
        }

        public class CollectionConstrainedByClass<TInput, TCollection>
            where TCollection : ConstraintClass<IEnumerable<TInput>>
        {
        }

        public class CollectionOfStrings : ConstraintClass<IEnumerable<string>>
        {
        }

        public class Constrained<T1, T2>
            where T2 : IConstraint<T1>
        {
        }

        public class ConstrainedWithGenericParameter<T1, T2>
            where T1 : T2
        {
        }

        public class ConstraintClass<T>
        {
        }

        public class COther : IOtherSimpleInterface
        {
        }

        public class COtherNestedConstrainted<T1, T2> : IDoubleGenericWithInModifier<T1, T2>
            where T2 : ISingleGenericWithOutModifier<IOtherSimpleInterface>
        {
        }

        public class CReversed<T2, T1> : IDouble<T1, T2>
        {
        }

        public class CSimple : ISimpleInterface
        {
        }

        public class MultiConstrained<T1, T2>
            where T1 : IEquatable<int>
            where T2 : IConstrainedConstraint<T1>
        {
        }

        public class SameTypes<TA, TB> : IDouble<TA, INested<IDouble<TB, TA>>>
        {
        }

        public class SameTypes<TA, TB, TC> : IDouble<INested<TA>, INested<IDouble<TB, TC>>>
        {
        }

        public class Wrapper<T>
        {
        }

        public interface IBaseGeneric<TDerived>
            where TDerived : BaseGenericImplementation<TDerived>, new()
        {
        }

        public class SelfReferenceConsumer<TSource> : IBaseGeneric<TSource>
            where TSource : BaseGenericImplementation<TSource>, new()
        {
        }

        public abstract class BaseGenericImplementation<TDerived>
        {
        }

        public class DerivedSelfReferencing : BaseGenericImplementation<DerivedSelfReferencing>
        {
        }
    }
}