using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac.Core;

namespace Autofac.Tests
{
    [TestFixture]
    public class NamedParameterTests
    {
        class A
        {
        }

        class B : A { }

        class C
        {
            public C(A a)
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
                .GetConstructor(new Type[] { typeof(A) })
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
