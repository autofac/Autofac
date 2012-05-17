using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Extras.Profiling;
using NUnit.Framework;

namespace Autofac.Extras.Tests.Profiling
{
    [TestFixture]
    public class ProfilingModuleTests
    {
        [Test]
        public void WhenAComponentIsActivated_ItsActivationCountIsRecorded()
        {
            var builder = new ContainerBuilder();
            var registration = RegistrationBuilder.ForType<object>().CreateRegistration();
            builder.RegisterComponent(registration);
            builder.RegisterModule(new ProfilingModule());
            var container = builder.Build();
            var profile = container.Resolve<IContainerProfile>();

            container.ResolveComponent(registration, Enumerable.Empty<Parameter>());
            container.ResolveComponent(registration, Enumerable.Empty<Parameter>());

            var info = profile.GetComponent(registration.Id);
            Assert.AreEqual(2, info.ActivationCount);
        }

        [Test]
        public void WhenAComponentDependsOnAnother_TheDependencyIsRecorded()
        {
            var builder = new ContainerBuilder();
            var registrationTo = RegistrationBuilder.ForType<object>().CreateRegistration();
            var registrationFrom = RegistrationBuilder.ForDelegate(
                    (c, p) => c.Resolve<object>().ToString()
                ).CreateRegistration();
            builder.RegisterComponent(registrationTo);
            builder.RegisterComponent(registrationFrom);
            builder.RegisterModule(new ProfilingModule());
            var container = builder.Build();
            var profile = container.Resolve<IContainerProfile>();

            container.Resolve<string>();

            var info = profile.GetComponent(registrationFrom.Id);

            IEnumerable<Guid> dependencies;
            Assert.IsTrue(info.TryGetDependencies(out dependencies));
            Assert.AreEqual(registrationTo.Id, dependencies.Single());
        }

        [Test]
        public void WhenAComponentDependsOnAnActivatedSingleton_TheDependencyIsRecorded()
        {
            var builder = new ContainerBuilder();
            var registrationTo = RegistrationBuilder.ForType<object>().SingleInstance().CreateRegistration();
            var registrationFrom = RegistrationBuilder.ForDelegate(
                    (c, p) => c.Resolve<object>().ToString()
                ).CreateRegistration();
            builder.RegisterComponent(registrationTo);
            builder.RegisterComponent(registrationFrom);
            builder.RegisterModule(new ProfilingModule());
            var container = builder.Build();
            var profile = container.Resolve<IContainerProfile>();

            container.Resolve<object>();
            container.Resolve<string>();

            var info = profile.GetComponent(registrationFrom.Id);

            IEnumerable<Guid> dependencies;
            Assert.IsTrue(info.TryGetDependencies(out dependencies));
            Assert.AreEqual(registrationTo.Id, dependencies.Single());
        }
    }
}
