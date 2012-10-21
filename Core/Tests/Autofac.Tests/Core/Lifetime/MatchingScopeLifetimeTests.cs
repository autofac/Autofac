using System;
using Autofac.Core;
using Autofac.Core.Lifetime;
using NUnit.Framework;

namespace Autofac.Tests.Core.Lifetime
{
    [TestFixture]
    public class MatchingScopeLifetimeTests
    {
        [Test]
        public void ObjectBasedMatch_WhenNoMatchingScopeIsPresent_TheExceptionMessageIncludesTheTag()
        {
            var container = new Container();
            const string tag = "abcdefg";
            var msl = new MatchingScopeLifetime(tag);
            var rootScope = (ISharingLifetimeScope)container.Resolve<ILifetimeScope>();

            var ex = Assert.Throws<DependencyResolutionException>(() => msl.FindScope(rootScope));
            Assert.That(ex.Message.Contains(tag));
        }

        [Test]
        public void ObjectBasedMatch_WhenProvidedTagIsNull_ExceptionThrown()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => new MatchingScopeLifetime((object)null));

            Assert.That(exception.ParamName, Is.EqualTo("lifetimeScopeTagToMatch"));
        }

        [Test]
        public void ObjectBasedMatch_MatchesAgainstTaggedScope()
        {
            const string tag = "Tag";
            var msl = new MatchingScopeLifetime(tag);
            var container = new Container();
            var lifetimeScope = (ISharingLifetimeScope)container.BeginLifetimeScope(tag);

            Assert.That(msl.FindScope(lifetimeScope), Is.EqualTo(lifetimeScope));
        }

        [Test]
        public void ExpressionBasedMatch_WhenNoMatchingScopeIsPresent_TheExceptionMessageIncludesTheExpression()
        {
            var container = new Container();
            var msl = new MatchingScopeLifetime(tag => "ABC".Equals(tag));
            var rootScope = (ISharingLifetimeScope)container.Resolve<ILifetimeScope>();

            var ex = Assert.Throws<DependencyResolutionException>(() => msl.FindScope(rootScope));
            Assert.That(ex.Message.Contains("\"ABC\".Equals(tag)"));
        }

        [Test]
        public void ExpressionBasedMatch_WhenProvidedExpressionIsNull_ExceptionThrown()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new MatchingScopeLifetime(null));
            Assert.That(exception.ParamName, Is.EqualTo("matchExpression"));
        }

        [Test]
        public void ExpressionBasedMatch_MatchesAgainstMultipleScopes()
        {
            const string tag1 = "Tag1";
            const string tag2 = "Tag2";

            var msl = new MatchingScopeLifetime(tag => tag.Equals(tag1) || tag.Equals(tag2));
            var container = new Container();

            var tag1Scope = (ISharingLifetimeScope)container.BeginLifetimeScope(tag1);
            Assert.That(msl.FindScope(tag1Scope), Is.EqualTo(tag1Scope));

            var tag2Scope = (ISharingLifetimeScope)container.BeginLifetimeScope(tag2);
            Assert.That(msl.FindScope(tag2Scope), Is.EqualTo(tag2Scope));
        }
    }
}
