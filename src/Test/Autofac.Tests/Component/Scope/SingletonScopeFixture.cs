using Autofac.Component;
using Autofac.Component.Scope;
using NUnit.Framework;

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
