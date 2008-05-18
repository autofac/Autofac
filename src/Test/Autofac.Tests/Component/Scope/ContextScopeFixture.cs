using Autofac.Component;
using Autofac.Component.Scope;
using NUnit.Framework;

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
			Assert.IsTrue(target.DuplicateForNewContext(out newScope));
			Assert.IsNotNull(newScope);
			Assert.AreNotSame(target, newScope);
		}
	}
}
