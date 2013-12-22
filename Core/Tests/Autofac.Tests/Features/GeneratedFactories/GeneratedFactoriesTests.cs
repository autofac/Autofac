using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
using NUnit.Framework;

namespace Autofac.Tests.Features.GeneratedFactories
{
    [TestFixture]
    public class GeneratedFactoriesTests
    {
        public class A<T>
        {
            public T P { get; private set; }

            public delegate A<T> Factory(T p);

            public A(T p)
            {
                P = p;
            }
        }

        [Test]
        public void CreateGenericFromFactoryDelegate()
        {
            var cb = new ContainerBuilder();

            cb.RegisterType<A<string>>();
            cb.RegisterGeneratedFactory<A<string>.Factory>(new TypedService(typeof(A<string>)));

            var container = cb.Build();

            var factory = container.Resolve<A<string>.Factory>();
            Assert.IsNotNull(factory);

            var s = "Hello!";
            var a = factory(s);
            Assert.IsNotNull(a);
            Assert.AreEqual(s, a.P);
        }

        [Test]
        public void CreateGenericFromFactoryDelegateImpliedServiceType()
        {
            var cb = new ContainerBuilder();

            cb.RegisterType<A<string>>();
            cb.RegisterGeneratedFactory<A<string>.Factory>();

            var container = cb.Build();

            var factory = container.Resolve<A<string>.Factory>();
            Assert.IsNotNull(factory);

            var s = "Hello!";
            var a = factory(s);
            Assert.IsNotNull(a);
            Assert.AreEqual(s, a.P);
        }

        public class QuoteService
        {
            public decimal GetQuote(string symbol)
            {
                return 2m;
            }
        }

        public class Shareholding
        {
            public delegate Shareholding Factory(string symbol, uint holding);

            public Shareholding(string symbol, uint holding, QuoteService qs)
            {
                Symbol = symbol;
                Holding = holding;
                _qs = qs;
            }

            private QuoteService _qs;

            public string Symbol { get; private set; }

            public uint Holding { get; set; }

            public decimal Quote()
            {
                return _qs.GetQuote(Symbol) * Holding;
            }
        }

        [Test]
        public void ShareholdingExample()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<QuoteService>();

            builder.RegisterType<Shareholding>();

            builder.RegisterGeneratedFactory<Shareholding.Factory>(
                new TypedService(typeof(Shareholding)));

            var container = builder.Build();

            var shareholdingFactory = container.Resolve<Shareholding.Factory>();

            var shareholding = shareholdingFactory.Invoke("ABC", 1234);

            Assert.AreEqual("ABC", shareholding.Symbol);
            Assert.AreEqual(1234, shareholding.Holding);
            Assert.AreEqual(1234m * 2, shareholding.Quote());
        }

        [Test]
        public void ShareholdingExampleMatchingFuncParametersByType()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<QuoteService>();

            builder.RegisterType<Shareholding>();

            builder.RegisterGeneratedFactory<Func<string, uint, Shareholding>>(
                new TypedService(typeof(Shareholding)));

            var container = builder.Build();

            var shareholdingFactory = container.Resolve<Func<string, uint, Shareholding>>();

            var shareholding = shareholdingFactory.Invoke("ABC", 1234);

            Assert.AreEqual("ABC", shareholding.Symbol);
            Assert.AreEqual(1234, shareholding.Holding);
            Assert.AreEqual(1234m * 2, shareholding.Quote());
        }

        public class StringHolder
        {
            public delegate StringHolder Factory();
            public string S;
        }

        [Test]
        public void RespectsContexts()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<StringHolder>()
                .InstancePerLifetimeScope();
            builder.RegisterGeneratedFactory<StringHolder.Factory>(new TypedService(typeof(StringHolder)));

            var container = builder.Build();

            container.Resolve<StringHolder>().S = "Outer";
            var inner = container.BeginLifetimeScope();
            inner.Resolve<StringHolder>().S = "Inner";

            var outerFac = container.Resolve<StringHolder.Factory>();
            Assert.AreEqual("Outer", outerFac().S);

            var innerFac = inner.Resolve<StringHolder.Factory>();
            Assert.AreEqual("Inner", innerFac().S);
        }

        [Test]
        public void CanSetParmeterMappingToPositional()
        {
            var builder = new ContainerBuilder();

            int i0 = 32, i0Actual = 0, i1 = 67, i1Actual = 0;

            builder.Register<object>((c, p) =>
            {
                i0Actual = p.Positional<int>(0);
                i1Actual = p.Positional<int>(1);
                return new object();
            });

            builder.RegisterGeneratedFactory<Func<int, int, object>>()
                .PositionalParameterMapping();

            var container = builder.Build();

            var generated = container.Resolve<Func<int, int, object>>();

            generated(i0, i1);

            Assert.AreEqual(i0, i0Actual);
            Assert.AreEqual(i1, i1Actual);
        }

        [Test]
        public void CanNameGeneratedFactories()
        {
            var builder = new ContainerBuilder();

            builder.RegisterGeneratedFactory<Func<object>>().Named<Func<object>>("object-factory");

            var container = builder.Build();

            var of = container.ResolveNamed<Func<object>>("object-factory");

            Assert.IsNotNull(of);
        }

        // This became necessary because by default the char[] constructor of string
        // is chosen in the presence of implicit collection support.
        public class HasCharIntCtor
        {
            public string Str;

            public HasCharIntCtor(char c, int i)
            {
                Str = new string(c, i);
            }
        }

        [Test]
        public void CanAutoGenerateFactoriesFromFuncs()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<HasCharIntCtor>();

            var container = builder.Build();

            var sf = container.Resolve<Func<char, int, HasCharIntCtor>>();
            var str = sf('a', 3).Str;

            Assert.AreEqual("aaa", str);
        }

        public delegate string CharCountStringFactory(char c, int count);

        [Test]
        public void CanAutoGenerateFactoriesFromCustomDelegateTypes()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<string>();

            var container = builder.Build();

            var sf = container.Resolve<CharCountStringFactory>();
            var str = sf('a', 3);

            Assert.AreEqual("aaa", str);
        }

        [Test]
        public void WillNotAutoGenerateFactoriesWhenProductNotRegistered()
        {
            var builder = new ContainerBuilder();

            var container = builder.Build();

            Assert.IsFalse(container.IsRegistered<Func<char, int, string>>());
        }

        [Test]
        public void CreateGenericFromNongenericFactoryDelegate()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<A<string>>();
            builder.RegisterGeneratedFactory(typeof(A<string>.Factory), new TypedService(typeof(A<string>)));

            var container = builder.Build();

            var factory = container.Resolve<A<string>.Factory>();
            Assert.IsNotNull(factory);

            var s = "Hello!";
            var a = factory(s);
            Assert.IsNotNull(a);
            Assert.AreEqual(s, a.P);

        }

        [Test]
        public void CreateGenericFromNongenericFactoryDelegateImpliedServiceType()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<A<string>>();
            builder.RegisterGeneratedFactory(typeof(A<string>.Factory));

            var container = builder.Build();

            var factory = container.Resolve<A<string>.Factory>();
            Assert.IsNotNull(factory);

            var s = "Hello!";
            var a = factory(s);
            Assert.IsNotNull(a);
            Assert.AreEqual(s, a.P);

        }

        [Test]
        public void WhenMultipleProductsAreRegistered_MultipleFactoriesCanBeResolved()
        {
            object o1 = new object(), o2 = new object();

            var builder = new ContainerBuilder();
            builder.RegisterInstance(o1);
            builder.RegisterInstance(o2);
            var container = builder.Build();

            var factories = container.Resolve<IEnumerable<Func<object>>>();

            Assert.AreEqual(2, factories.Count());
            Assert.IsTrue(factories.Any(f => f() == o1));
            Assert.IsTrue(factories.Any(f => f() == o2));
        }

        [Test]
        public void ResolvingAutoGeneratedFactoryByName_ReturnsProductsByName()
        {
            object o = new object();

            var builder = new ContainerBuilder();
            builder.RegisterInstance(o).Named<object>("o");
            var container = builder.Build();

            var fac = container.ResolveNamed<Func<object>>("o");

            Assert.AreSame(o, fac());
        }

        [Test(Description = "Issue #269: An object with duplicate constructor parameter types should fail Func resolution from an auto-generated factory.")]
        public void DuplicateConstructorParameterTypesDoNotResolve()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<DuplicateConstructorParameterTypes>();
            var container = builder.Build();
            var func = container.Resolve<Func<int, int, string, DuplicateConstructorParameterTypes>>();
            Assert.Throws<DependencyResolutionException>(() => func(1, 2, "3"));
        }

        [Test(Description = "Issue #269: If you register a custom Func for an object with duplicate parameters it should override the auto-generated factory.")]
        public void FactoryOverrideCanBeRegisteredForDuplicateParameterTypes()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<DuplicateConstructorParameterTypes>();
            builder.Register<Func<int, int, string, DuplicateConstructorParameterTypes>>(ctx => (a, b, c) => new DuplicateConstructorParameterTypes(a, b, c));
            var container = builder.Build();
            var func = container.Resolve<Func<int, int, string, DuplicateConstructorParameterTypes>>();
            var obj = func(1, 2, "3");
            Assert.AreEqual(1, obj.A);
            Assert.AreEqual(2, obj.B);
            Assert.AreEqual("3", obj.C);
        }

        [Test(Description = "Issue #269: If you register a specific delegate type for an object with duplicate parameters it should work and not throw an exception.")]
        public void SpecificDelegateCanBeRegisteredForDuplicateParameterTypes()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<DuplicateConstructorParameterTypes>();
            builder.RegisterGeneratedFactory<DuplicateConstructorParameterTypes.Factory>(new TypedService(typeof(DuplicateConstructorParameterTypes)));
            var container = builder.Build();
            var func = container.Resolve<DuplicateConstructorParameterTypes.Factory>();
            var obj = func(1, 2, "3");
            Assert.AreEqual(1, obj.A);
            Assert.AreEqual(2, obj.B);
            Assert.AreEqual("3", obj.C);
        }

        private class DuplicateConstructorParameterTypes
        {
            public delegate DuplicateConstructorParameterTypes Factory(int a, int b, string c);

            public int A { get; set; }

            public int B { get; set; }

            public string C { get; set; }

            // This constructor should not be able to be resolved into a Func<int, int, string, DuplicateConstructorParameterTypes>
            // because of the redundant types in the constructor.
            public DuplicateConstructorParameterTypes(int a, int b, string c)
            {
                this.A = a;
                this.B = b;
                this.C = c;
            }
        }
    }
}
