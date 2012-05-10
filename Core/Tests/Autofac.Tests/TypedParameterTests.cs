using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac.Core;

namespace Autofac.Tests
{
    [TestFixture]
    public class TypedParameterTests
    {
        public class A
        {
        }

        public class B : A { }

        public class C
        {
            public C(A a)
            {
            }
        }

        [Test]
        public void MatchesIdenticallyTypedParameter()
        {
            var param = AParamOfCConstructor();

            var typedParam = new TypedParameter(typeof(A), new A());

            Func<object> vp;
            Assert.IsTrue(typedParam.CanSupplyValue(param, new Container(), out vp));
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
        public void DoesNotMatchPolymorphicallyTypedParameter()
        {
            var param = AParamOfCConstructor();

            var typedParam = new TypedParameter(typeof(B), new B());

            Func<object> vp;
            Assert.IsFalse(typedParam.CanSupplyValue(param, new Container(), out vp));
        }

        [Test]
        public void DoesNotMatchUnrelatedParameter()
        {
            var param = AParamOfCConstructor();

            var typedParam = new TypedParameter(typeof(string), "Yo!");

            Func<object> vp;
            Assert.IsFalse(typedParam.CanSupplyValue(param, new Container(), out vp));
        }

		[Test]
		public void FromWorksJustLikeTheConstructor()
		{
			var param = TypedParameter.From(new B());
			Assert.AreSame(typeof(B), param.Type);
		}
    }
}
