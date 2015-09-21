using Autofac.Core.Registration;
using Xunit;
using System;
using System.Linq;
using System.Reflection;

namespace Autofac.Test.Features.OpenGenerics
{
    public class ComplexGenericsTests
    {
        // ReSharper disable UnusedTypeParameter, InconsistentNaming
        public interface IDouble<T2, T3> { }
        public class CReversed<T2, T1> : IDouble<T1, T2> { }

        public interface INested<T> { }

        public interface INested<T, D> { }
        public class Wrapper<T> { }
        public class CNested<T> : INested<Wrapper<T>> { }

        public class CNestedDerived<T> : CNested<T> { }

        public class CNestedDerivedReversed<TX, TY> : IDouble<TY, INested<Wrapper<TX>>> { }
        public class SameTypes<TA, TB> : IDouble<TA, INested<IDouble<TB, TA>>> { }

        public class SameTypes<TA, TB, TC> : IDouble<INested<TA>, INested<IDouble<TB, TC>>> { }
        // ReSharper restore UnusedTypeParameter, InconsistentNaming

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
        public void NestedGenericClassesCanBeResolved()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(CNestedDerived<>)).As(typeof(CNested<>));
            var container = cb.Build();

            var nest = container.Resolve<CNested<Wrapper<string>>>();
            Assert.IsType<CNestedDerived<Wrapper<string>>>(nest);
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
        public void CanResolveConcreteTypesThatReorderImplementedInterfaceParameters()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(CReversed<,>));
            var container = cb.Build();

            var self = container.Resolve<CReversed<int, string>>();
            Assert.IsType<CReversed<int, string>>(self);
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
        public void TheSamePlaceholderWithThreeGenericParametersTypeCanAppearMultipleTimesInTheService()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(SameTypes<,,>)).As(typeof(SameTypes<,,>).GetTypeInfo().ImplementedInterfaces.ToArray());
            var container = cb.Build();

            var compl = container.Resolve<IDouble<INested<string>, INested<IDouble<int, long>>>>();
            Assert.IsType<SameTypes<string, int, long>>(compl);
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
        public void WhenTheSameTypeAppearsMultipleTimesInTheImplementationMappingItMustAlsoInTheService()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(SameTypes<,>)).As(typeof(SameTypes<,>).GetInterfaces());
            var container = cb.Build();

            Assert.Throws<ComponentNotRegisteredException>(() =>
                container.Resolve<IDouble<decimal, INested<IDouble<string, int>>>>());
        }

        // ReSharper disable UnusedTypeParameter
        public interface IConstraint<T> { }

        public class Constrained<T1, T2> where T2 : IConstraint<T1> { }
        // ReSharper restore UnusedTypeParameter

        [Fact]
        public void CanResolveComponentWithNestedConstraintViaInterface()
        {
            var builder = new ContainerBuilder();

            builder.RegisterGeneric(typeof(Constrained<,>));

            var container = builder.Build();

            Assert.True(container.IsRegistered<Constrained<int, IConstraint<int>>>());
        }

        // ReSharper disable UnusedTypeParameter
        public interface IConstraintWithAddedArgument<T2, T1> : IConstraint<T1> { }
        // ReSharper restore UnusedTypeParameter

        [Fact]
        public void CanResolveComponentWhenConstrainedArgumentIsGenericTypeWithMoreArgumentsThanGenericConstraint()
        {
            var builder = new ContainerBuilder();

            builder.RegisterGeneric(typeof(Constrained<,>));

            var container = builder.Build();

            Assert.True(container.IsRegistered<Constrained<int, IConstraintWithAddedArgument<string, int>>>());
        }

        // ReSharper disable UnusedTypeParameter
        public interface IConstrainedConstraint<T> where T : IEquatable<int> { }

        public interface IConstrainedConstraintWithAddedArgument<T1, T2> : IConstrainedConstraint<T2> where T2 : IEquatable<int> { }

        public interface IConstrainedConstraintWithOnlyAddedArgument<T1> : IConstrainedConstraintWithAddedArgument<T1, int> { }

        public class MultiConstrained<T1, T2> where T1 : IEquatable<int> where T2 : IConstrainedConstraint<T1> { }
        // ReSharper restore UnusedTypeParameter

        [Fact]
        public void CanResolveComponentWhenConstraintsAreNested()
        {
            var builder = new ContainerBuilder();

            builder.RegisterGeneric(typeof(MultiConstrained<,>));

            var container = builder.Build();

            Assert.True(container.IsRegistered<MultiConstrained<int, IConstrainedConstraintWithOnlyAddedArgument<string>>>());
        }
    }
}

namespace FluentValidation
{
    interface IValidator<T> { }
    class AbstractValidator<T> : IValidator<T> { }
}

namespace CompanyA
{
    interface IValidator<T> { } // Please note that this name is the same as FluentValidation's IValidator<T>
    class CompositeValidator<T> : FluentValidation.AbstractValidator<T>, IValidator<T> { }
}

namespace CompanyB
{
    interface IValidatorSomeOtherName<T> { } // This is NOT the same name.
    class CompositeValidator<T> : FluentValidation.AbstractValidator<T>, IValidatorSomeOtherName<T> { }
}

namespace Autofac.Test.Features.OpenGenerics
{
    public class ComplexGenericsWithNamepsaceTests
    {
        [Fact]
        public void CanResolveByGenericInterface()
        {
            var builder = new ContainerBuilder();

            builder.RegisterGeneric(typeof(CompanyA.CompositeValidator<>)).As(typeof(CompanyA.IValidator<>));

            var container = builder.Build();

            var validator = container.Resolve<CompanyA.IValidator<int>>();
            Assert.IsType<CompanyA.CompositeValidator<int>>(validator);
        }
    }
}