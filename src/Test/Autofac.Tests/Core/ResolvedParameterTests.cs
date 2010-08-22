using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Autofac.Core;

namespace Autofac.Tests.Core
{
    [TestFixture]
    public class ResolvedParameterTests
    {
        [Test]
        public void ResolvesParameterValueFromContext()
        {
            var cb = new ContainerBuilder();
            cb.RegisterInstance((object)'a').Named<char>("character");
            cb.RegisterType<string>()
                .UsingConstructor(typeof(char), typeof(int))
                .WithParameter(new TypedParameter(typeof(int), 5))
                .WithParameter(new ResolvedParameter(
                    (pi, ctx) => pi.ParameterType == typeof(char),
                    (pi, ctx) => ctx.ResolveNamed<char>("character")));
            var c = cb.Build();
            var s = c.Resolve<string>();
            Assert.AreEqual("aaaaa", s);
        }

// ReSharper disable UnusedTypeParameter
        interface ISomething<T> { }
// ReSharper restore UnusedTypeParameter

        class ConcreteSomething<T> : ISomething<T> { }

        class SomethingDecorator<T> : ISomething<T>
        {
            public ISomething<T> Decorated { get; private set; }

            public SomethingDecorator(ISomething<T> decorated)
            {
                Decorated = decorated;
            }
        }

        [Test]
        public void CanConstructDecoratorChainFromOpenGenericTypes()
        {
            var builder = new ContainerBuilder();

            builder.RegisterGeneric(typeof(ConcreteSomething<>));

            var decoratedSomethingArgument = new ResolvedParameter(
                (pi, c) => pi.Name == "decorated",
                (pi, c) => c.Resolve(typeof(ConcreteSomething<>).MakeGenericType(
                                pi.ParameterType.GetGenericArguments())));

            builder.RegisterGeneric(typeof(SomethingDecorator<>))
                .As(typeof(ISomething<>))
                .WithParameter(decoratedSomethingArgument);

            var container = builder.Build();

            var concrete = container.Resolve<ISomething<int>>();

            Assert.IsInstanceOf<SomethingDecorator<int>>(concrete);
            Assert.IsInstanceOf<ConcreteSomething<int>>(((SomethingDecorator<int>)concrete).Decorated);
        }

        [Test]
        public void ResolvedParameterForNamedServiceResolvesNamedService()
        {
            var cb = new ContainerBuilder();
            cb.RegisterInstance((object)'a').Named<char>("character");
            cb.RegisterType<string>()
                .UsingConstructor(typeof(char), typeof(int))
                .WithParameter(new TypedParameter(typeof(int), 5))
                .WithParameter(ResolvedParameter.ForNamed<char>("character"));
            var c = cb.Build();
            var s = c.Resolve<string>();
            Assert.AreEqual("aaaaa", s);
        }

        [Test]
        public void AResolvedParameterForAKeyedServiceMatchesParametersOfTheServiceTypeWhenTheKeyedServiceIsAvailable()
        {
            var k = new object();
            var builder = new ContainerBuilder();
            builder.RegisterInstance((object) 'a').Keyed<char>(k);
            var container = builder.Build();
            var rp = ResolvedParameter.ForKeyed<char>(k);
            var cp = GetCharParameter();
            Func<object> vp;
            Assert.That(rp.CanSupplyValue(cp, container, out vp));
        }

        [Test]
        public void AResolvedParameterForAKeyedServiceDoesNotMatcheParametersOfTheServiceTypeWhenTheKeyedServiceIsUnavailable()
        {
            var rp = ResolvedParameter.ForKeyed<char>(new object());
            var cp = GetCharParameter();
            Func<object> vp;
            var canSupply = rp.CanSupplyValue(cp, Container.Empty, out vp);
            Assert.That(canSupply, Is.False);
        }

        static ParameterInfo GetCharParameter()
        {
            return typeof(string)
                .GetConstructor(new[] {typeof (char), typeof (int)})
                .GetParameters()
                .First();
        }
    }
}
