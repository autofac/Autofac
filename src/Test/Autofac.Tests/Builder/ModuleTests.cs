using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using NUnit.Framework;
using Autofac.Core;
using Autofac.Core.Registration;

namespace Autofac.Tests.Builder
{
    [TestFixture]
    public class ModuleTests
    {
        class ObjectModule : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                builder.RegisterInstance(new object());
            }
        }

        [Test]
        public void LoadsRegistrations()
        {
            var cr = new ComponentRegistry();
            new ObjectModule().Configure(cr);
            Assert.IsTrue(cr.IsRegistered(new TypedService(typeof(object))));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DetectsNullComponentRegistryArgument()
        {
            new ObjectModule().Configure(null);
        }

        class AttachingModule : Module
        {
            public IList<IComponentRegistration> Registrations = new List<IComponentRegistration>();

            protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
            {
                base.AttachToComponentRegistration(componentRegistry, registration);
                Registrations.Add(registration);
            }
        }

        [Test]
        public void AttachesToRegistrations()
        {
            var attachingModule = new AttachingModule();
            Assert.AreEqual(0, attachingModule.Registrations.Count);

            var builder = new ContainerBuilder();
            builder.RegisterType(typeof(object));
            builder.RegisterModule(attachingModule);
            builder.RegisterInstance("Hello!");
            
            var container = builder.Build();

            Assert.AreEqual(container.ComponentRegistry.Registrations.Count(), attachingModule.Registrations.Count);
        }
    }
}
