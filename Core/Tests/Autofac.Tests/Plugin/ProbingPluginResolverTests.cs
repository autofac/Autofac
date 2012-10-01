using System;
using System.IO;
using System.Reflection;
using Autofac.Plugin;
using Moq;
using NUnit.Framework;

namespace Autofac.Tests.Plugin
{
    [TestFixture]
    public class ProbingPluginResolverTests
    {
        [Test]
        public void Resolve_PassesFalseAsThrowOnErrorToAssemblyGetType()
        {
            bool? result = null;
            var assembly = GetAssembly((name, throwOnError) => { result = throwOnError; return typeof(object); });
            var resolver = new ProbingPluginResolver(assemblyString => assembly, "Unknown");

            resolver.Resolve(typeof(IAppDomainSetup));

            Assert.That(result.Value, Is.False);
        }

        [Test]
        public void ResolveOptional_PassesFalseAsThrowOnErrorToAssemblyGetType()
        {
            bool? result = null;
            var assembly = GetAssembly((name, throwOnError) => { result = throwOnError; return typeof(object); });
            var resolver = new ProbingPluginResolver(assemblyString => assembly, "Unknown");

            resolver.ResolveOptional(typeof(IAppDomainSetup));

            Assert.That(result.Value, Is.False);
        }

        [Test]
        public void Resolve_PassesTypeNameMinusIToAssemblyGetType()
        {
            string result = null;
            var assembly = GetAssembly((name, throwOnError) => { result = name; return typeof(object); });
            var resolver = new ProbingPluginResolver(assemblyString => assembly, "Unknown");

            resolver.Resolve(typeof(IAppDomainSetup));

            Assert.That(result, Is.EqualTo("System.AppDomainSetup"));
        }

        [Test]
        public void ResolveOptional_PassesTypeNameMinusIToAssemblyGetType()
        {
            string result = null;
            var assembly = GetAssembly((name, throwOnError) => { result = name; return typeof(object); });
            var resolver = new ProbingPluginResolver(assemblyString => assembly, "Unknown");

            resolver.ResolveOptional(typeof(IAppDomainSetup));

            Assert.That(result, Is.EqualTo("System.AppDomainSetup"));
        }

        [Test]
        public void Resolve_PassesCorrectAssemblyNameToAssemblyLoader()
        {
            string result = null;
            var resolver = new ProbingPluginResolver(assemblyString => { result = assemblyString; return typeof(object).Assembly; }, "Unknown");

            resolver.Resolve(typeof(IAppDomainSetup));

            var name = new AssemblyName(result);

            Assert.That(name.Name, Is.EqualTo("Autofac.Unknown"));
        }

        [Test]
        public void ResolveOptional_PassesCorrectAssemblyNameToAssemblyLoader()
        {
            string result = null;
            var resolver = new ProbingPluginResolver(assemblyString => { result = assemblyString; return typeof(object).Assembly; }, "Unknown");

            resolver.ResolveOptional(typeof(IAppDomainSetup));

            var name = new AssemblyName(result);

            Assert.That(name.Name, Is.EqualTo("Autofac.Unknown"));
        }

        [Test]
        public void Resolve_WhenPlatformSpecificAssemblyNotFound_ThrowsInvalidOperationException()
        {
            var resolver = new ProbingPluginResolver(assemblyString => { throw new FileNotFoundException(); }, PlatformPlugin.FullPlatformName);

            Assert.Throws<InvalidOperationException>(() => resolver.Resolve(typeof(IAppDomainSetup)));
        }

        [Test]
        public void ResolveOptional_WhenPlatformSpecificAssemblyNotFound_ReturnsNull()
        {
            var resolver = new ProbingPluginResolver(assemblyString => { throw new FileNotFoundException(); }, PlatformPlugin.FullPlatformName);

            Assert.That(resolver.ResolveOptional(typeof(IAppDomainSetup)), Is.Null);
        }

        [Test]
        public void Resolve_WhenPluginNotFound_ThrowsInvalidOperationException()
        {
            var resolver = new ProbingPluginResolver(assemblyString => typeof(object).Assembly, PlatformPlugin.FullPlatformName);

            var exception = Assert.Throws<InvalidOperationException>(
                () => resolver.Resolve(typeof(IDisposable)));

            Assert.That(exception.Message, Is.StringContaining(typeof(IDisposable).FullName));
        }

        [Test]
        public void ResolveOptional_WhenPluginNotFound_ReturnsNull()
        {
            var resolver = new ProbingPluginResolver(assemblyString => typeof(object).Assembly, PlatformPlugin.FullPlatformName);

            var result = resolver.ResolveOptional(typeof(IDisposable));

            Assert.That(result, Is.Null);
        }

        [Test]
        public void Resolve_KeepsProbingUntilAdapterIsFound()
        {
            var count = 0;
            var resolver = new ProbingPluginResolver(assemblyString => count++ == 0 ? null : typeof(object).Assembly, "Unknown", PlatformPlugin.FullPlatformName);

            var result = resolver.Resolve(typeof(IAppDomainSetup));

            Assert.That(result, Is.InstanceOf<AppDomainSetup>());
        }

        [Test]
        public void ResolveOptional_KeepsProbingUntilAdapterIsFound()
        {
            var count = 0;
            var resolver = new ProbingPluginResolver(assemblyString => count++ == 0 ? null : typeof(object).Assembly, "Unknown", PlatformPlugin.FullPlatformName);

            var result = resolver.ResolveOptional(typeof(IAppDomainSetup));

            Assert.That(result, Is.InstanceOf<AppDomainSetup>());
        }

        [Test]
        public void Resolve_SameTypeMultipleTimes_ReturnsSameInstances()
        {
            var resolver = new ProbingPluginResolver(assemblyString => typeof(object).Assembly, PlatformPlugin.FullPlatformName);

            var result1 = resolver.Resolve(typeof(IAppDomainSetup));
            var result2 = resolver.Resolve(typeof(IAppDomainSetup));

            Assert.That(result1, Is.SameAs(result2));
        }

        [Test]
        public void ResolveOptional_SameTypeMultipleTimes_ReturnsSameInstances()
        {
            var resolver = new ProbingPluginResolver(assemblyString => typeof(object).Assembly, PlatformPlugin.FullPlatformName);

            var result1 = resolver.ResolveOptional(typeof(IAppDomainSetup));
            var result2 = resolver.ResolveOptional(typeof(IAppDomainSetup));

            Assert.That(result1, Is.SameAs(result2));
        }

        public static Assembly GetAssembly(Func<string, bool, Type> action)
        {
            var mock = new Mock<CustomAssembly>();
            mock.Setup(m => m.GetType(It.IsAny<string>(), It.IsAny<bool>())).Returns(action);
            return mock.Object;
        }

        public class CustomAssembly : Assembly
        {
        }
    }
}
