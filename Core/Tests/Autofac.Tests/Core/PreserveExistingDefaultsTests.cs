using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Autofac.Tests.Core
{
    /// <summary>
    /// Tests to verify registration ordering and resolution when used in conjunction with
    /// preserving existing registration defaults.
    /// </summary>
    [TestFixture]
    public class PreserveExistingDefaultsTests
    {
        [Test]
        public void ContainerScope_ComplexConsumerServicesResolve()
        {
            // This is an all-around "integration test" with property injection,
            // constructor injection, different service types, some overrides, etc.
            // all in one - just to see it all working together.
            var builder = new ContainerBuilder();
            builder.RegisterInstance("s1").PreserveExistingDefaults();
            builder.Register(c => 7);
            builder.Register(c => 8).PreserveExistingDefaults();
            var obj = new object();
            builder.RegisterInstance(obj);
            builder.Register(c => new object());

            builder.RegisterType<ComplexConsumer>().PropertiesAutowired();

            var container = builder.Build();
            var consumer = container.Resolve<ComplexConsumer>();
            Assert.AreEqual("s1", consumer.Text, "The string value was not resolved properly.");
            Assert.AreEqual(7, consumer.Number, "The number value was not resolved properly.");
            Assert.IsNotNull(consumer.Value, "The object value was not injected.");
            Assert.AreNotSame(obj, consumer.Value, "The object value was not resolved properly.");
        }

        [Test]
        public void ContainerScope_DefaultServiceRegistrationUsingPreservation()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance("s1").PreserveExistingDefaults();
            var container = builder.Build();
            Assert.AreEqual("s1", container.Resolve<string>());
        }

        [Test]
        public void ContainerScope_MultipleServiceRegistrationsUsingPreservation()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance("s1").PreserveExistingDefaults();
            builder.RegisterInstance("s2").PreserveExistingDefaults();
            builder.RegisterInstance("s3").PreserveExistingDefaults();
            var container = builder.Build();
            Assert.AreEqual("s1", container.Resolve<string>());
        }

        [Test]
        public void ContainerScope_PreserveDoesNotOverrideDefault()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance("s1");
            builder.RegisterInstance("s2").PreserveExistingDefaults();
            var container = builder.Build();
            Assert.AreEqual("s1", container.Resolve<string>());
        }

        [Test]
        public void ContainerScope_PreserveSupportsIEnumerable()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance("s1").PreserveExistingDefaults();
            builder.RegisterInstance("s2").PreserveExistingDefaults();
            builder.RegisterInstance("s3").PreserveExistingDefaults();
            var container = builder.Build();
            var resolved = container.Resolve<IEnumerable<string>>();
            Assert.AreEqual(3, resolved.Count(), "The wrong number of components were resolved.");
            Assert.IsTrue(resolved.Any(s => s == "s1"), "The first service wasn't present.");
            Assert.IsTrue(resolved.Any(s => s == "s2"), "The second service wasn't present.");
            Assert.IsTrue(resolved.Any(s => s == "s3"), "The third service wasn't present.");
        }

        [Test]
        public void ContainerScope_SimpleRegistrationPreservationStillAllowsOverride()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance("s1");
            builder.RegisterInstance("s2").PreserveExistingDefaults();
            builder.RegisterInstance("s3");
            var container = builder.Build();
            Assert.AreEqual("s3", container.Resolve<string>());
        }

        [Test]
        [Ignore("Issue #272")]
        public void NestedScope_ComplexConsumerServicesResolve()
        {
            // This is an all-around "integration test" with property injection,
            // constructor injection, different service types, some overrides, etc.
            // all in one - just to see it all working together.
            var builder = new ContainerBuilder();
            builder.RegisterInstance("s1").PreserveExistingDefaults();
            builder.Register(c => 7);
            builder.Register(c => 8).PreserveExistingDefaults();
            var obj = new object();
            builder.RegisterInstance(obj);
            builder.Register(c => new object());

            var container = builder.Build();
            var scope = container.BeginLifetimeScope(
                b =>
                {
                    b.RegisterInstance("s2").PreserveExistingDefaults();
                    b.Register(c => 9);
                    b.RegisterInstance(obj);
                    builder.Register(c => new object()).PreserveExistingDefaults();
                    b.RegisterType<ComplexConsumer>().PropertiesAutowired();
                });

            var consumer = scope.Resolve<ComplexConsumer>();
            Assert.AreEqual("s1", consumer.Text, "The string value was not resolved properly.");
            Assert.AreEqual(9, consumer.Number, "The number value was not resolved properly.");
            Assert.IsNotNull(consumer.Value, "The object value was not injected.");
            Assert.AreSame(obj, consumer.Value, "The object value was not resolved properly.");
        }

        [Test]
        public void NestedScope_DefaultRegistrationCanOverrideParent()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance("s1");
            var container = builder.Build();
            var scope = container.BeginLifetimeScope(b => b.RegisterInstance("s2"));
            Assert.AreEqual("s2", scope.Resolve<string>());
        }

        [Test]
        [Ignore("Issue #272")]
        public void NestedScope_PreserveDefaultsCanFallBackToNestedParent()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance("s1");
            var container = builder.Build();
            var scope1 = container.BeginLifetimeScope(b => b.RegisterInstance("s2").PreserveExistingDefaults());
            var scope2 = scope1.BeginLifetimeScope(b => b.RegisterInstance("s3"));
            var scope3 = scope2.BeginLifetimeScope(b => b.RegisterInstance("s4").PreserveExistingDefaults());
            Assert.AreEqual("s3", scope3.Resolve<string>());
        }

        [Test]
        [Ignore("Issue #272")]
        public void NestedScope_PreserveDefaultsCanFallBackToParent()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance("s1");
            var container = builder.Build();
            var scope = container.BeginLifetimeScope(b => b.RegisterInstance("s2").PreserveExistingDefaults());
            Assert.AreEqual("s1", scope.Resolve<string>());
        }

        [Test]
        [Ignore("Issue #272")]
        public void NestedScope_PreserveDefaultsCanFallBackToParentThroughMultipleNesting()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance("s1");
            var container = builder.Build();
            var scope1 = container.BeginLifetimeScope(b => b.RegisterInstance("s2").PreserveExistingDefaults());
            var scope2 = scope1.BeginLifetimeScope(b => b.RegisterInstance("s3").PreserveExistingDefaults());
            var scope3 = scope2.BeginLifetimeScope(b => b.RegisterInstance("s4").PreserveExistingDefaults());
            Assert.AreEqual("s1", scope3.Resolve<string>());
        }

        [Test]
        public void NestedScope_PreserveDefaultsCanFallbackToDefaultNestedRegistrations()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance("s1");
            var container = builder.Build();
            var scope = container.BeginLifetimeScope(
                b =>
                {
                    b.RegisterInstance("s2");
                    b.RegisterInstance("s3").PreserveExistingDefaults();
                });
            Assert.AreEqual("s2", scope.Resolve<string>());
        }

        [Test]
        public void NestedScope_PreserveStillSupportsIEnumerable()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance("s1").PreserveExistingDefaults();
            builder.RegisterInstance("s2").PreserveExistingDefaults();
            builder.RegisterInstance("s3").PreserveExistingDefaults();
            var container = builder.Build();
            var scope = container.BeginLifetimeScope(b => b.RegisterInstance("s4").PreserveExistingDefaults());
            var resolved = scope.Resolve<IEnumerable<string>>();
            Assert.AreEqual(4, resolved.Count(), "The wrong number of components were resolved.");
            Assert.IsTrue(resolved.Any(s => s == "s1"), "The first service wasn't present.");
            Assert.IsTrue(resolved.Any(s => s == "s2"), "The second service wasn't present.");
            Assert.IsTrue(resolved.Any(s => s == "s3"), "The third service wasn't present.");
            Assert.IsTrue(resolved.Any(s => s == "s4"), "The fourth service wasn't present.");
        }

        private class ComplexConsumer
        {
            public int Number { get; private set; }
            public string Text { get; private set; }
            public object Value { get; set; }

            public ComplexConsumer(int number, string text)
            {
                this.Number = number;
                this.Text = text;
            }
        }
    }
}
