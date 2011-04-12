using System.Linq;
using Autofac.Builder;
using Autofac.Integration.Wcf;
using NUnit.Framework;
using Autofac.Util;
using Autofac.Core;

namespace Autofac.Tests.Integration.Wcf
{
	[TestFixture]
	public class AutofacInstanceContextFixture
	{
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

		[Test]
		public void InstancesDisposed()
		{
			var builder = new ContainerBuilder();
			builder.RegisterType<DisposeTracker>();
			var container = builder.Build();
			IComponentRegistration registration;
			container.ComponentRegistry.TryGetRegistration(new TypedService(typeof(DisposeTracker)), out registration);
			var context = new AutofacInstanceContext(container);
			var disposable = (DisposeTracker)context.ResolveComponent(registration, Enumerable.Empty<Parameter>());
			Assert.IsFalse(disposable.IsDisposedPublic);
			context.Dispose();
			Assert.IsTrue(disposable.IsDisposedPublic);
		}
	}
}
