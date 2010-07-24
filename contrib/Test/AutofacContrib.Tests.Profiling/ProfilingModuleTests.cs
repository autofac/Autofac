using System.Linq;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using AutofacContrib.Profiling;
using NUnit.Framework;

namespace AutofacContrib.Tests.Profiling
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

            container.Resolve(registration, Enumerable.Empty<Parameter>());
            container.Resolve(registration, Enumerable.Empty<Parameter>());

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

            CollectionAssert.AreEquivalent(new[] { registrationTo.Id }, info.Dependencies);
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

            CollectionAssert.AreEquivalent(new[] { registrationTo.Id }, info.Dependencies);
        }
    }
}
