using Autofac.Core.Registration;
using NUnit.Framework;

namespace Autofac.Tests.Features.OpenGenerics
{
    [TestFixture]
    public class ComplexGenericsTests
    {
        public interface IDouble<T2, T3> { }
        public class CReversed<T2, T1> : IDouble<T1, T2> { }

        public interface INested<T> { }
        public class Wrapper<T> { }
        public class CNested<T> : INested<Wrapper<T>> { }

        public class CNestedDerived<T> : CNested<T> { }

        public class CNestedDerivedReversed<TX, TY> : IDouble<TY, INested<Wrapper<TX>>> { }

        public class SameTypes<TA, TB> : IDouble<TA, INested<IDouble<TB, TA>>> { }

        [Test]
        public void NestedGenericInterfacesCanBeResolved()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(CNested<>)).As(typeof(INested<>));
            var container = cb.Build();

            var nest = container.Resolve<INested<Wrapper<string>>>();
            Assert.IsInstanceOf<CNested<string>>(nest);
        }

        [Test]
        public void NestedGenericClassesCanBeResolved()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(CNestedDerived<>)).As(typeof(CNested<>));
            var container = cb.Build();

            var nest = container.Resolve<CNested<Wrapper<string>>>();
            Assert.IsInstanceOf<CNestedDerived<Wrapper<string>>>(nest);
        }

        [Test]
        public void CanResolveImplementationsWhereTypeParametersAreReordered()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(CReversed<,>)).As(typeof(IDouble<,>));
            var container = cb.Build();

            var repl = container.Resolve<IDouble<int, string>>();
            Assert.IsInstanceOf<CReversed<string, int>>(repl);
        }

        [Test]
        public void CanResolveConcreteTypesThatReorderImplementedInterfaceParameters()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(CReversed<,>));
            var container = cb.Build();

            var self = container.Resolve<CReversed<int, string>>();
            Assert.IsInstanceOf<CReversed<int, string>>(self);
        }

        [Test]
        public void TestNestingAndReversingSimplification()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(CNestedDerivedReversed<,>)).As(typeof(IDouble<,>));
            var container = cb.Build();

            var compl = container.Resolve<IDouble<int, INested<Wrapper<string>>>>();
            Assert.IsInstanceOf<CNestedDerivedReversed<string, int>>(compl);
        }

        [Test]
        public void TestReversingWithoutNesting()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(CReversed<,>)).As(typeof(IDouble<,>));
            var container = cb.Build();

            var compl = container.Resolve<IDouble<int, INested<Wrapper<string>>>>();
            Assert.IsInstanceOf<CReversed<INested<Wrapper<string>>, int>>(compl);
        }

        [Test]
        public void TestWithSameTypes()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(SameTypes<,>)).As(typeof(SameTypes<,>).GetInterfaces());
            var container = cb.Build();

            var compl = container.Resolve<IDouble<int, INested<IDouble<string, int>>>>();
            Assert.IsInstanceOf<SameTypes<int, string>>(compl);
        }

        [Test]
        public void TestWithSameTypesError()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(SameTypes<,>)).As(typeof(SameTypes<,>).GetInterfaces());
            var container = cb.Build();

            Assert.Throws<ComponentNotRegisteredException>(() =>
                container.Resolve<IDouble<decimal, INested<IDouble<string, int>>>>());
        }
    }
}
