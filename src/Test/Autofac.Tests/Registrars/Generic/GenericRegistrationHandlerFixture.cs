using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Autofac.Component;
using Autofac.Registrars.Generic;
using Autofac.Builder;

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
                new EventHandler<ActivatedEventArgs>[] { },
                (d, a, s, o) => new Registration(d, a, s, o)));

			var x = c.Resolve<I<int>>();
			var x2 = c.Resolve<I<int>>();

			Assert.IsNotNull(x);
			Assert.AreSame(x, x2);
			Assert.AreEqual(typeof(A<>), x.GetType().GetGenericTypeDefinition());

			c.Dispose();

			Assert.IsTrue(((DisposeTracker)x).IsDisposed);
		}
		
		[Test]
		public void GenericRegistrationsInSubcontextOverrideRootContext()
		{
			var builder = new ContainerBuilder();
			builder.RegisterGeneric(typeof(List<>)).As(typeof(ICollection<>)).FactoryScoped();
			var container = builder.Build();
			var inner = container.CreateInnerContainer();
			var innerBuilder = new ContainerBuilder();
			innerBuilder.RegisterGeneric(typeof(LinkedList<>)).As(typeof(ICollection<>)).FactoryScoped();
			innerBuilder.Build(inner);
			
			var list = inner.Resolve<ICollection<int>>();
			Assert.IsInstanceOfType(typeof(LinkedList<int>), list);
		}
		
		[Test]
		public void SingletonGenericComponentsResolvedInSubcontextStickToParent()
		{
			var builder = new ContainerBuilder();
			builder.RegisterGeneric(typeof(List<>)).As(typeof(ICollection<>));
			var container = builder.Build();
			var inner = container.CreateInnerContainer();
			
			var innerList = inner.Resolve<ICollection<int>>();
			var outerList = container.Resolve<ICollection<int>>();
			Assert.AreSame(innerList, outerList);
		}
		
	}
}
