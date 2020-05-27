using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Registration;
using Xunit;

namespace Autofac.Test
{
    public class ModuleTests
    {
        internal class ObjectModule : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                builder.RegisterInstance(new Service1());
            }
        }

        [Fact]
        public void LoadsRegistrations()
        {
            IComponentRegistryBuilder builder = Factory.CreateEmptyComponentRegistryBuilder();
            new ObjectModule().Configure(builder);
            var registry = builder.Build();

            Assert.True(registry.IsRegistered(new TypedService(typeof(Service1))));
        }

        [Fact]
        public void DetectsNullComponentRegistryArgument()
        {
            Assert.Throws<ArgumentNullException>(() => new ObjectModule().Configure(null));
        }

        internal class AttachingModule : Module
        {
            public IList<IComponentRegistration> Registrations { get; set; } = new List<IComponentRegistration>();

            protected override void AttachToComponentRegistration(IComponentRegistryBuilder componentRegistry, IComponentRegistration registration)
            {
                base.AttachToComponentRegistration(componentRegistry, registration);
                Registrations.Add(registration);
            }
        }

        [Fact]
        public void AttachesToRegistrations()
        {
            var attachingModule = new AttachingModule();
            Assert.Equal(0, attachingModule.Registrations.Count);

            var builder = new ContainerBuilder();
            builder.RegisterType(typeof(Service1));
            builder.RegisterModule(attachingModule);
            builder.RegisterInstance("Hello!");

            var container = builder.Build();

            Assert.Equal(container.ComponentRegistry.Registrations.Count(), attachingModule.Registrations.Count);
        }

        [Fact]
        public void AttachesToRegistrationsInScope()
        {
            var attachingModule = new AttachingModule();
            Assert.Equal(0, attachingModule.Registrations.Count);

            var builder = new ContainerBuilder();
            builder.RegisterModule(attachingModule);

            using (var container = builder.Build())
            using (var scope = container.BeginLifetimeScope(c => c.RegisterType(typeof(Service1))))
            {
                var expected = container.ComponentRegistry.Registrations.Count() + scope.ComponentRegistry.Registrations.Count();
                Assert.Equal(expected, attachingModule.Registrations.Count);
            }
        }

        [Fact]
        public void AttachesToRegistrationsInNestedScope()
        {
            var attachingModule = new AttachingModule();
            Assert.Equal(0, attachingModule.Registrations.Count);

            var builder = new ContainerBuilder();
            builder.RegisterModule(attachingModule);

            using (var container = builder.Build())
            using (var outerScope = container.BeginLifetimeScope(c => c.RegisterType(typeof(Service1))))
            using (var innerScope = outerScope.BeginLifetimeScope(c => c.RegisterType(typeof(Service2))))
            {
                var expected = container.ComponentRegistry.Registrations.Count()
                    + outerScope.ComponentRegistry.Registrations.Count() + innerScope.ComponentRegistry.Registrations.Count();
                Assert.Equal(expected, attachingModule.Registrations.Count);
            }
        }

        [Fact]
        public void ModifiedScopesHaveTheirOwnDelegate()
        {
            var attachingModule = new AttachingModule();
            Assert.Equal(0, attachingModule.Registrations.Count);

            var builder = new ContainerBuilder();
            builder.RegisterModule(attachingModule);

            using (var container = builder.Build())
            {
                Assert.NotNull(container.ComponentRegistry.Properties[MetadataKeys.RegisteredPropertyKey]);
                using (container.BeginLifetimeScope(c =>
                {
                    c.RegisterCallback(outerBuilder =>
                    {
                        Assert.Equal(
                            container.ComponentRegistry.Properties[MetadataKeys.RegisteredPropertyKey],
                            outerBuilder.Properties[MetadataKeys.RegisteredPropertyKey]);

                        outerBuilder.Registered += (s, e) => { };

                        Assert.NotEqual(
                            container.ComponentRegistry.Properties[MetadataKeys.RegisteredPropertyKey],
                            outerBuilder.Properties[MetadataKeys.RegisteredPropertyKey]);
                    });
                    c.RegisterCallback(outerBuilder =>
                    {
                        Assert.Equal(
                            container.ComponentRegistry.Properties[MetadataKeys.RegistrationSourceAddedPropertyKey],
                            outerBuilder.Properties[MetadataKeys.RegistrationSourceAddedPropertyKey]);

                        outerBuilder.RegistrationSourceAdded += (s, e) => { };

                        Assert.NotEqual(
                            container.ComponentRegistry.Properties[MetadataKeys.RegisteredPropertyKey],
                            outerBuilder.Properties[MetadataKeys.RegisteredPropertyKey]);
                    });

                    c.RegisterType(typeof(Service1));
                }))
                {
                }
            }
        }

        internal class ModuleExposingThisAssembly : Module
        {
            public Assembly ModuleThisAssembly
            {
                get
                {
                    return ThisAssembly;
                }
            }
        }

        [Fact]
        public void TheAssemblyExposedByThisAssemblyIsTheOneContainingTheConcreteModuleClass()
        {
            var module = new ModuleExposingThisAssembly();
            Assert.Same(typeof(ModuleExposingThisAssembly).GetTypeInfo().Assembly, module.ModuleThisAssembly);
        }

        internal class ModuleIndirectlyExposingThisAssembly : ModuleExposingThisAssembly
        {
        }

        [Fact]
        public void IndirectlyDerivedModulesCannotUseThisAssembly()
        {
            var module = new ModuleIndirectlyExposingThisAssembly();
            Assert.Throws<InvalidOperationException>(() => { var unused = module.ModuleThisAssembly; });
        }

        internal class PropertySetModule : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                // Increment a counter to show use of existing properties.
                var count = (int)builder.Properties["count"];
                count++;
                builder.Properties["count"] = count;

                if (!builder.Properties.ContainsKey("prop"))
                {
                    // If Add is called twice, it'll throw.
                    builder.Properties.Add("prop", "value");
                }
            }
        }

        [Fact]
        public void CanUseBuilderPropertyBag()
        {
            var builder = new ContainerBuilder();
            builder.Properties["count"] = 0;

            // Registering the module twice helps test whether
            // we can do things like detecting duplicates.
            builder.RegisterModule<PropertySetModule>();
            builder.RegisterModule<PropertySetModule>();

            var container = builder.Build();
            Assert.Equal(2, container.ComponentRegistry.Properties["count"]);
            Assert.Equal("value", builder.Properties["prop"]);
        }

        private class Service1
        {
        }

        private class Service2
        {
        }

        private class Service3
        {
        }
    }
}
