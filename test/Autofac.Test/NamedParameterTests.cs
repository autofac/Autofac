using System;
using System.Linq;
using System.Reflection;
using Autofac.Core;
using Xunit;

namespace Autofac.Test
{
    public class NamedParameterTests
    {
        public class A
        {
        }

        public class B : A
        {
        }

        public class C
        {
            public C(A a)
            {
            }
        }

        [Fact]
        public void MatchesIdenticallyNamedParameter()
        {
            var param = AParamOfCConstructor();

            var namedParam = new NamedParameter("a", new A());

            Func<object> vp;
            Assert.True(namedParam.CanSupplyValue(param, new Container(), out vp));
        }

        private static System.Reflection.ParameterInfo AParamOfCConstructor()
        {
            var param = typeof(C)
                .GetTypeInfo()
                .DeclaredConstructors
                .Single()
                .GetParameters()
                .First();
            return param;
        }

        [Fact]
        public void DoesNotMatchDifferentlyNamedParameter()
        {
            var param = AParamOfCConstructor();

            var namedParam = new NamedParameter("b", new B());

            Func<object> vp;
            Assert.False(namedParam.CanSupplyValue(param, new Container(), out vp));
        }
    }
}
