using Autofac.Core;
using Autofac.Core.Lifetime;
using NUnit.Framework;

namespace Autofac.Tests.Core.Lifetime
{
    [TestFixture]
    public class MatchingScopeLifetimeTests
    {
        [Test]
        public void WhenNoMatchingScopeIsPresent_TheExceptionMessageIncludesTheTag()
        {
            var container = new Container();
            const string tag = "abcdefg";
            var msl = new MatchingScopeLifetime(tag);
            var rootScope = (ISharingLifetimeScope)container.Resolve<ILifetimeScope>();

            var ex = Assert.Throws<DependencyResolutionException>(() => msl.FindScope(rootScope));
            Assert.That(ex.Message.Contains(tag));
        }
    }
}
