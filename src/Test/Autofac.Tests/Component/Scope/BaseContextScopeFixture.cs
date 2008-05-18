using System;
using Autofac.Component.Scope;
using NUnit.Framework;

namespace Autofac.Tests.Component.Scope
{
	public abstract class BaseContextScopeFixture
	{
		protected abstract ContainerScope CreateTarget();

		[Test]
		public void InstanceNotAvailableBeforeSet()
		{
			ContainerScope target = CreateTarget();
			Assert.IsFalse(target.InstanceAvailable);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void GetInstanceShouldThrowWithoutInstance()
		{
			ContainerScope target = CreateTarget();
			target.GetInstance();
		}

		[Test]
		public void SetInstance()
		{
			object instance = new object();

			ContainerScope target = CreateTarget();
			target.SetInstance(instance);

			Assert.AreSame(instance, target.GetInstance());
			Assert.IsTrue(target.InstanceAvailable);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void SecondSetInstanceThrows()
		{
			ContainerScope target = new ContainerScope();
			target.SetInstance(new object());
			target.SetInstance(new object());
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SetInstanceNull()
		{
			ContainerScope target = new ContainerScope();
			target.SetInstance(null);
		}

	}
}
