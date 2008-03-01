using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac.Builder;

namespace Autofac.Tests.Builder
{
    [TestFixture]
    public class ModuleFixture
    {
        class ObjectModule : Module
        {
            protected override void Load()
            {
                this.Register(new object());
            }
        }

        [Test]
        public void LoadsRegistrations()
        {
            var container = new Container();
            new ObjectModule().Configure(container);
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

            protected override void AttachToComponentRegistration(IContainer container, IComponentRegistration registration)
            {
                base.AttachToComponentRegistration(container, registration);
                Registrations.Add(registration);
            }
        }

        [Test]
        public void AttachesToRegistrations()
        {
            var attachingModule = new AttachingModule();
            Assert.AreEqual(0, attachingModule.Registrations.Count);

            var builder = new ContainerBuilder();
            builder.Register<object>();
            builder.RegisterModule(attachingModule);
            builder.Register("Hello!");
            
            var container = builder.Build();

            Assert.AreEqual(container.ComponentRegistrations.Count(), attachingModule.Registrations.Count);
        }
    }
}
