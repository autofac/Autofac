using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Autofac.Component;
using Autofac.Registrars.Generic;

namespace Autofac.Tests.Registrars.Generic
{
	[TestFixture]
	public class GenericRegistrationHandlerFixture
	{
		interface I<T> { }

		class A<T> : DisposeTracker, I<T> { }

		[Test]
		public void RegistrationProvided()
		{
			var c = new Container();
            c.AddRegistrationSource(new GenericRegistrationHandler(
                new Service[] { new TypedService(typeof(I<>)) },
                typeof(A<>),
                InstanceOwnership.Container,
                InstanceScope.Singleton,
                new EventHandler<ActivatingEventArgs>[] { },
                new EventHandler<ActivatedEventArgs>[] { }));

			var x = c.Resolve<I<int>>();
			var x2 = c.Resolve<I<int>>();

			Assert.IsNotNull(x);
			Assert.AreSame(x, x2);
			Assert.AreEqual(typeof(A<>), x.GetType().GetGenericTypeDefinition());

			c.Dispose();

			Assert.IsTrue(((DisposeTracker)x).IsDisposed);
		}
	}
}
