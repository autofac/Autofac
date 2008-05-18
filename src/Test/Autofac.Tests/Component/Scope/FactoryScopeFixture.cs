using System;
using Autofac.Component;
using Autofac.Component.Scope;
using NUnit.Framework;

namespace Autofac.Tests.Component.Scope
{
    [TestFixture]
    public class FactoryScopeFixture
    {
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetInstanceShouldThrow()
        {
            FactoryScope target = new FactoryScope();
            target.GetInstance();
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetInstanceShouldThrowEvenWithInstanceSet()
        {
            FactoryScope target = new FactoryScope();
            target.SetInstance(new object());
            target.GetInstance();
        }

        [Test]
        public void NeverInstanceAvailable()
        {
            FactoryScope target = new FactoryScope();
            target.SetInstance(new object());
            Assert.IsFalse(target.InstanceAvailable);
        }

        [Test]
        public void SetInstanceMultipleTimesOkay()
        {
            FactoryScope target = new FactoryScope();
            target.SetInstance(new object());
            target.SetInstance(new object());
        }

		[Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetInstanceNull()
        {
            FactoryScope target = new FactoryScope();
            target.SetInstance(null);
        }

		[Test]
		public void NewScopeSupportedWithSameInstance()
		{
			var target = new ContainerScope();
			IScope newScope;
			Assert.IsTrue(target.DuplicateForNewContext(out newScope));
			Assert.IsNotNull(newScope);
			Assert.AreNotSame(target, newScope);
		}
    }
}
