using Autofac.Builder;
using NUnit.Framework;
using System;

namespace Autofac.Tests.GeneratedFactories
{
    [TestFixture]
    public class ContainerBuilderExtensionsFixture
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
            var builder = new ContainerBuilder();

            builder.Register<A<string>>().WithScope(InstanceScope.Factory);
            builder.RegisterGeneratedFactory<A<string>.Factory>(new TypedService(typeof(A<string>)));

            var container = builder.Build();

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
            var builder = new ContainerBuilder();

            builder.Register<A<string>>().WithScope(InstanceScope.Factory);
            builder.RegisterGeneratedFactory<A<string>.Factory>();

            var container = builder.Build();

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

            builder.Register<QuoteService>();

            builder.Register<Shareholding>()
              .WithScope(InstanceScope.Factory);

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

            builder.Register<QuoteService>();

            builder.Register<Shareholding>()
              .FactoryScoped();

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
            builder.Register<StringHolder>()
                .WithScope(InstanceScope.Container);
            builder.RegisterGeneratedFactory<StringHolder.Factory>(new TypedService(typeof(StringHolder)));

            var outer = builder.Build();
            outer.Resolve<StringHolder>().S = "Outer";
            var inner = outer.CreateInnerContainer();
            inner.Resolve<StringHolder>().S = "Inner";

            var outerFac = outer.Resolve<StringHolder.Factory>();
            Assert.AreEqual("Outer", outerFac().S);

            var innerFac = inner.Resolve<StringHolder.Factory>();
            Assert.AreEqual("Inner", innerFac().S);
        }
    }
}
