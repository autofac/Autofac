using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using NUnit.Framework;

namespace Autofac.Tests.Builder
{
    [TestFixture]
    public class ModuleFixture
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
            var container = new Container();
            new ObjectModule().Configure(container.ComponentRegistry);
            Assert.IsTrue(container.IsRegistered<object>());
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DetectsNullContainer()
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
