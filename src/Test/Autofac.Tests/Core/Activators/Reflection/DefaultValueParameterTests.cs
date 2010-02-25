using System;
using System.Linq;
using System.Reflection;
using Autofac.Core.Activators.Reflection;
using NUnit.Framework;

namespace Autofac.Tests.Core.Activators.Reflection
{
    class HasDefaultValues
    {
        public HasDefaultValues(string s, string t = "Hello")
        {
        }
    }

    [TestFixture]
    public class DefaultValueParameterTests
    {
        static ParameterInfo GetTestParameter(string name)
        {
            return typeof(HasDefaultValues).GetConstructors().Single()
                .GetParameters().Where(pi => pi.Name == name).Single();
        }

        [Test]
        public void DoesNotProvideValueWhenNoDefaultAvailable()
        {
            var dvp = new DefaultValueParameter();
            Func<object> vp;
            Assert.IsFalse(dvp.CanSupplyValue(GetTestParameter("s"), Autofac.Core.Container.Empty, out vp));
        }

        [Test]
        public void ProvidesValueWhenDefaultInitialiserPresent()
        {
            var dvp = new DefaultValueParameter();
            var u = GetTestParameter("t");
            Func<object> vp;
            Assert.IsTrue(dvp.CanSupplyValue(u, Autofac.Core.Container.Empty, out vp));
            Assert.AreEqual("Hello", vp());
        }
    }
}
