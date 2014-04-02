using System;
using Autofac.Configuration;
using NUnit.Framework;

namespace Autofac.Tests.Configuration
{
    [TestFixture]
    public class AppSettingsModuleFixture
    {
        [Test]
        public void CanMapSimplePropertyFromAppSettings()
        {
            var module = new StringModule();
            var builder = new ContainerBuilder();
            builder.RegisterType<ComponentWithStringParameter>();
            builder.RegisterModule(new AppSettingsModule(new[] {module}));
            var container = builder.Build();

            var component = container.Resolve<ComponentWithStringParameter>();

            Assert.That(component.StringParameter, Is.EqualTo("Autofac rocks!"));
        }

        [Test]
        public void CanMapComplexPropertyFromAppSettings()
        {
            var module = new UriModule();
            var builder = new ContainerBuilder();
            builder.RegisterType<ComponentWithUriParameter>();
            builder.RegisterModule(new AppSettingsModule(new[] { module }));
            var container = builder.Build();

            var component = container.Resolve<ComponentWithUriParameter>();

            Assert.That(component.UriParameter, Is.EqualTo(new Uri("http://autofac.org")));
        }
    }

    public class StringModule : Module
    {
        public string StringSetting { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ComponentWithStringParameter>().WithParameter("stringParameter", StringSetting);
        }
    }

    public class UriModule : Module
    {
        public Uri UriSetting { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ComponentWithUriParameter>().WithParameter("uriParameter", UriSetting);
        }
    }

    public class ComponentWithStringParameter
    {
        public string StringParameter { get; private set; }

        public ComponentWithStringParameter(string stringParameter)
        {
            StringParameter = stringParameter;
        }
    }

    public class ComponentWithUriParameter
    {
        public Uri UriParameter { get; private set; }

        public ComponentWithUriParameter(Uri uriParameter)
        {
            UriParameter = uriParameter;
        }
    }
}
