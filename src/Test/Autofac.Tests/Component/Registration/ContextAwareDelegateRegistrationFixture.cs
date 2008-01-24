using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Autofac.Component.Registration;

namespace Autofac.Tests.Component.Registration
{
	[TestFixture]
	public class ContextAwareDelegateRegistrationFixture
	{
		public class A<T>
		{
			public T P { get; private set; }

			public delegate A<T> Factory(T p);

			public A(T p)
			{
				P = p;
			}
		}

		[Test]
		public void CreateFromStrongFactory()
		{
			var target = new ContextAwareDelegateRegistration(
				typeof(A<int>.Factory),
				(c, p) => new A<int>(p.Get<int>("p")));

			var cont = new Container();
			cont.RegisterComponent(target);

			var factory = cont.Resolve<A<int>.Factory>();
			Assert.IsNotNull(factory);

			var i = 2;
			var a = factory(i);
			Assert.IsNotNull(a);
			Assert.AreEqual(i, a.P);
		}
	}
}
