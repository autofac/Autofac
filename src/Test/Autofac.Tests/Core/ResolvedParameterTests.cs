using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac;
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
                    (pi, ctx) => ctx.Resolve<char>("character")));
            var c = cb.Build();
            var s = c.Resolve<string>();
            Assert.AreEqual("aaaaa", s);
        }

        interface ISomething<T> { }

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
    }
}
