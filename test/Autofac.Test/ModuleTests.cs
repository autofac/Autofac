using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                builder.RegisterInstance(new object());
            }
        }

        [Fact]
        public void LoadsRegistrations()
        {
            var cr = new ComponentRegistry();
            new ObjectModule().Configure(cr);
            Assert.True(cr.IsRegistered(new TypedService(typeof(object))));
        }

        [Fact]
        public void DetectsNullComponentRegistryArgument()
        {
            Assert.Throws<ArgumentNullException>(() => new ObjectModule().Configure(null));
        }

        internal class AttachingModule : Module
        {
            public IList<IComponentRegistration> Registrations { get; set; } = new List<IComponentRegistration>();

            protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
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
            builder.RegisterType(typeof(object));
            builder.RegisterModule(attachingModule);
            builder.RegisterInstance("Hello!");

            var container = builder.Build();

            Assert.Equal(container.ComponentRegistry.Registrations.Count(), attachingModule.Registrations.Count);
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
            Assert.Equal(2, builder.Properties["count"]);
            Assert.Equal("value", builder.Properties["prop"]);
        }
    }
}
