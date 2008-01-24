using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Autofac.Component.Scope;
using Autofac.Component;

namespace Autofac.Tests.Component.Scope
{
    [TestFixture]
    public class SingletonScopeFixture : BaseContextScopeFixture
    {
		protected override ContainerScope CreateTarget()
		{
			return new SingletonScope();
		}

		[Test]
		public void NewScopeNotSupported()
		{
			var target = new SingletonScope();
			IScope newScope;
			Assert.IsFalse(target.DuplicateForNewContext(out newScope));
			Assert.IsNull(newScope);
		}
    }
}
