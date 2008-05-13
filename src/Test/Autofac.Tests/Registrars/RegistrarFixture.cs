using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac.Builder;

namespace Autofac.Tests.Registrars
{
    [TestFixture]
    public class RegistrarFixture
    {
        [Test]
        public void RegistrationMadeWhenPredicateTrue()
        {
            var cb = new ContainerBuilder();
            cb.Register<object>().OnlyIf(c => true);
            var container = cb.Build();
            Assert.IsTrue(container.IsRegistered<object>());
        }

        [Test]
        public void RegistrationNotMadeWhenPredicateFalse()
        {
            var cb = new ContainerBuilder();
            cb.Register<object>().OnlyIf(c => false);
            var container = cb.Build();
            Assert.IsFalse(container.IsRegistered<object>());
        }

        [Test]
        public void ContainerPassedToPredicate()
        {
            var cb = new ContainerBuilder();
            IContainer passed = null;
            cb.Register<object>().OnlyIf(c => { passed = c; return true; });
            var container = cb.Build();
            Assert.IsNotNull(passed);
            Assert.AreSame(passed, container);
        }

        [Test]
        public void OneFalsePredicatePreventsRegistration()
        {
            var cb = new ContainerBuilder();
            cb.Register<object>().OnlyIf(c => true).OnlyIf(c => false).OnlyIf(c => true);
            var container = cb.Build();
            Assert.IsFalse(container.IsRegistered<object>());
        }

        [Test]
        public void AllTruePredicatesAllowsRegistration()
        {
            var cb = new ContainerBuilder();
            cb.Register<object>().OnlyIf(c => true).OnlyIf(c => true).OnlyIf(c => true);
            var container = cb.Build();
            Assert.IsTrue(container.IsRegistered<object>());
        }
    }
}
