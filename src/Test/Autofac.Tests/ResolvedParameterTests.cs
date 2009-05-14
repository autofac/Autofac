using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac.Builder;

namespace Autofac.Tests
{
    [TestFixture]
    public class ResolvedParameterTests
    {
        [Test]
        public void ResolvesParameterValueFromContext()
        {
            var cb = new ContainerBuilder();
            cb.Register('a').Named("character");
            cb.Register<string>().WithArguments(
                new TypedParameter(typeof(int), 5),
                new ResolvedParameter(
                    (pi, ctx) => pi.ParameterType == typeof(char),
                    (ctx) => ctx.Resolve<char>("character")));
            var c = cb.Build();
            var s = c.Resolve<string>();
            Assert.AreEqual("aaaaa", s);
        }
    }
}
