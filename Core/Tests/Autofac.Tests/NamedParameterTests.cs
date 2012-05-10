using System;
using System.Linq;
using NUnit.Framework;
using Autofac.Core;

namespace Autofac.Tests
{
    [TestFixture]
    public class NamedParameterTests
    {
        public class A
        {
        }

        public class B : A { }

        public class C
        {
            // ReSharper disable UnusedParameter.Local
            public C(A a)
            // ReSharper restore UnusedParameter.Local
            {
            }
        }

        [Test]
        public void MatchesIdenticallyNamedParameter()
        {
            var param = AParamOfCConstructor();

            var namedParam = new NamedParameter("a", new A());

            Func<object> vp;
            Assert.IsTrue(namedParam.CanSupplyValue(param, new Container(), out vp));
        }

        private static System.Reflection.ParameterInfo AParamOfCConstructor()
        {
            var param = typeof(C)
                .GetConstructor(new [] { typeof(A) })
                .GetParameters()
                .First();
            return param;
        }

        [Test]
        public void DoesNotMatchDifferentlyNamedParameter()
        {
            var param = AParamOfCConstructor();

            var namedParam = new NamedParameter("b", new B());

            Func<object> vp;
            Assert.IsFalse(namedParam.CanSupplyValue(param, new Container(), out vp));
        }
    }
}
