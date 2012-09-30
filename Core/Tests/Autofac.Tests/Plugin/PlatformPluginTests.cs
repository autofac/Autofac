using System;
using Autofac.Plugin;
using NUnit.Framework;

namespace Autofac.Tests.Plugin
{
    [TestFixture]
    public class PlatformPluginTests
    {
        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            PlatformPlugin.ResetResolver();
        }

        [Test]
        public void Resolve_ReturnsResultOfPluginResolverResolve()
        {
            var plugin = new object();
            var resolver = GetPluginResolver(type => plugin);
            PlatformPlugin.SetResolver(resolver);

            var result = PlatformPlugin.Resolve<object>();

            Assert.That(result, Is.SameAs(plugin));
        }

        [Test]
        public void ResolveOptional_ReturnsResultOfPluginResolverResolve()
        {
            var plugin = new object();
            var resolver = GetPluginResolver(type => plugin);
            PlatformPlugin.SetResolver(resolver);

            var result = PlatformPlugin.ResolveOptional<object>();

            Assert.That(result, Is.SameAs(plugin));
        }

        [Test]
        public void Resolve_WhenPluginResolverThrows_LetsExceptionPassThrough()
        {
            var expectedException = new Exception();
            var resolver = GetPluginResolver(type => { throw expectedException; });
            PlatformPlugin.SetResolver(resolver);

            var exception = Assert.Throws<Exception>(() => PlatformPlugin.Resolve<object>());
            Assert.That(exception, Is.SameAs(expectedException));
        }

        [Test]
        public void ResolveOptional_WhenPluginResolverThrows_LetsExceptionPassThrough()
        {
            var expectedException = new Exception();
            var resolver = GetPluginResolver(type => { throw expectedException; });
            PlatformPlugin.SetResolver(resolver);

            var exception = Assert.Throws<Exception>(() => PlatformPlugin.ResolveOptional<object>());
            Assert.That(exception, Is.SameAs(expectedException));
        }

        [Test]
        public void Resolve_WhenPluginResolverReturnsNull_ThrowsPlatformNotSupported()
        {
            var resolver = GetPluginResolver(type => null);
            PlatformPlugin.SetResolver(resolver);

            Assert.Throws<PlatformNotSupportedException>(() => PlatformPlugin.Resolve<object>());
        }

        [Test]
        public void ResolveOptional_WhenPluginResolverReturnsNull_ReturnsNull()
        {
            var resolver = GetPluginResolver(type => null);
            PlatformPlugin.SetResolver(resolver);

            Assert.That(PlatformPlugin.ResolveOptional<object>(), Is.Null);
        }

        static IPluginResolver GetPluginResolver(Func<Type, object> action)
        {
            return new MockPluginResolver(action);
        }
    }

    class MockPluginResolver : IPluginResolver
    {
        readonly Func<Type, object> _action;

        public MockPluginResolver(Func<Type, object> action)
        {
            _action = action;
        }

        public object Resolve(Type type)
        {
            return _action(type);
        }

        public object ResolveOptional(Type type)
        {
            return _action(type);
        }
    }
}
