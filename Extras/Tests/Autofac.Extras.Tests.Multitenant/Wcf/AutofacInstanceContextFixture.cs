using System;
using Autofac;
using Autofac.Util;
using Autofac.Extras.Multitenant.Wcf;
using NUnit.Framework;

namespace Autofac.Extras.Tests.Multitenant.Wcf
{
	[TestFixture]
	public class AutofacInstanceContextFixture
	{
		[Test(Description = "You can't create an instance context without a parent scope.")]
		public void Ctor_RequiresParentScope()
		{
			Assert.Throws<ArgumentNullException>(() => new AutofacInstanceContext(null));
		}

		[Test(Description = "Creating an instance context with a parent scope creates a child scope.")]
		public void Ctor_CreatesChildScope()
		{
			var container = new ContainerBuilder().Build();
			var context = new AutofacInstanceContext(container);
			Assert.IsNotNull(context.LifetimeScope, "The lifetime scope should be set.");
			Assert.AreNotSame(container, context.LifetimeScope, "The lifetime scope should not be identical to the one passed into the constructor.");
		}

		[Test(Description = "When the instance context gets disposed, service instances should also be disposed.")]
		public void Dispose_InstancesDisposed()
		{
			var builder = new ContainerBuilder();
			builder.RegisterType<DisposeTracker>();
			var container = builder.Build();

			var impData = new ServiceImplementationData()
			{
				ConstructorString = "TestService",
				ServiceTypeToHost = typeof(DisposeTracker),
				ImplementationResolver = l => l.Resolve<DisposeTracker>()
			};

			var context = new AutofacInstanceContext(container);
			var disposable = (DisposeTracker)context.Resolve(impData);
			Assert.IsFalse(disposable.IsDisposedPublic);
			context.Dispose();
			Assert.IsTrue(disposable.IsDisposedPublic);
		}

		[Test(Description = "You can't resolve a service implementation without the data about the resolution.")]
		public void Resolve_RequiresServiceImplementationData()
		{
			var context = new AutofacInstanceContext(new ContainerBuilder().Build());
			Assert.Throws<ArgumentNullException>(() => context.Resolve(null));
		}

		private class DisposeTracker : Disposable
		{
			public bool IsDisposedPublic
			{
				get
				{
					return this.IsDisposed;
				}
			}
		}
	}
}
