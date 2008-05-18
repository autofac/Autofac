using System;
using System.Collections.Generic;
using Autofac.Builder;
using Autofac.Component;
using Autofac.Component.Activation;
using Autofac.Registrars;
using Autofac.Registrars.Generic;
using NUnit.Framework;

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
                new DeferredRegistrationParameters(
                    InstanceOwnership.Container,
                    InstanceScope.Singleton,
                    new EventHandler<PreparingEventArgs>[] { },
                    new EventHandler<ActivatingEventArgs>[] { },
                    new EventHandler<ActivatedEventArgs>[] { },
                    (d, a, s, o) => new Registration(d, a, s, o)),
                new MostParametersConstructorSelector()));

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
		
		[Test]
		public void GenericCircularityAvoidedWithUsingContstructor()
		{
			var builder = new ContainerBuilder();
			builder.RegisterGeneric(typeof(List<>))
				.As(typeof(IEnumerable<>))
				.UsingConstructor(new Type[] {});
			var container = builder.Build();
			var list = container.Resolve<IEnumerable<int>>();
		}
	}
}
