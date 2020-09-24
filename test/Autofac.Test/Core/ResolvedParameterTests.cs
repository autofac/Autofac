// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Autofac.Core;
using Xunit;

namespace Autofac.Test.Core
{
    public class ResolvedParameterTests
    {
        [Fact]
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
            Assert.Equal("aaaaa", s);
        }

        public interface ISomething<T>
        {
        }

        public class ConcreteSomething<T> : ISomething<T>
        {
        }

        public class SomethingDecorator<T> : ISomething<T>
        {
            public ISomething<T> Decorated { get; private set; }

            public SomethingDecorator(ISomething<T> decorated)
            {
                Decorated = decorated;
            }
        }

        [Fact]
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

            Assert.IsType<SomethingDecorator<int>>(concrete);
            Assert.IsType<ConcreteSomething<int>>(((SomethingDecorator<int>)concrete).Decorated);
        }

        [Fact]
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
            Assert.Equal("aaaaa", s);
        }

        [Fact]
        public void AResolvedParameterForAKeyedServiceMatchesParametersOfTheServiceTypeWhenTheKeyedServiceIsAvailable()
        {
            var k = new object();
            var builder = new ContainerBuilder();
            builder.RegisterInstance((object)'a').Keyed<char>(k);
            var container = builder.Build();
            var rp = ResolvedParameter.ForKeyed<char>(k);
            var cp = GetCharParameter();
            Assert.True(rp.CanSupplyValue(cp, container, out Func<object> vp));
        }

        [Fact]
        public void AResolvedParameterForAKeyedServiceDoesNotMatcheParametersOfTheServiceTypeWhenTheKeyedServiceIsUnavailable()
        {
            var rp = ResolvedParameter.ForKeyed<char>(new object());
            var cp = GetCharParameter();
            var canSupply = rp.CanSupplyValue(cp, new ContainerBuilder().Build(), out Func<object> vp);
            Assert.False(canSupply);
        }

        private static ParameterInfo GetCharParameter()
        {
            return typeof(string)
                .GetConstructor(new[] { typeof(char), typeof(int) })
                .GetParameters()
                .First();
        }
    }
}
