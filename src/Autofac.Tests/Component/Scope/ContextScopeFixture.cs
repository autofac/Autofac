using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Autofac.Component.Scope;
using Autofac.Component;

namespace Autofac.Tests.Component.Scope
{
	[TestFixture]
	public class ContextScopeFixture : BaseContextScopeFixture
	{
		protected override ContainerScope CreateTarget()
		{
			return new ContainerScope();
		}

		[Test]
		public void NewScopeSupportedWithNewInstance()
		{
			var target = new ContainerScope();
			IScope newScope;
			Assert.IsTrue(target.TryDuplicateForNewContext(out newScope));
			Assert.IsNotNull(newScope);
			Assert.AreNotSame(target, newScope);
		}
	}
}
