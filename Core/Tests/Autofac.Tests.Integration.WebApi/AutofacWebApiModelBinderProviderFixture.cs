using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Autofac.Integration.WebApi;
using NUnit.Framework;

namespace Autofac.Tests.Integration.WebApi
{
    [TestFixture]
    public class AutofacWebApiModelBinderProviderFixture
    {
        [Test]
        public void ProviderIsRegisteredAsSingleInstance()
        {
            var container = BuildContainer();
            var provider1 = container.Resolve<ModelBinderProvider>();
            var provider2 = container.Resolve<ModelBinderProvider>();
            Assert.That(provider1, Is.SameAs(provider2));
        }

        [Test]
        public void ModelBindersAreRegistered()
        {
            var container = BuildContainer();
            var modelBinders = container.Resolve<IEnumerable<IModelBinder>>();
            Assert.That(modelBinders.Count(), Is.EqualTo(1));
        }

        [Test]
        public void ModelBinderHasDependenciesInjected()
        {
            var container = BuildContainer();
            var modelBinder = container.Resolve<IEnumerable<IModelBinder>>()
                .OfType<TestModelBinder>()
                .FirstOrDefault();
            Assert.That(modelBinder, Is.Not.Null);
            Assert.That(modelBinder.Dependency, Is.Not.Null);
        }

        [Test]
        public void ReturnsNullWhenModelBinderRegisteredWithoutMetadata()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<Dependency>().AsSelf();
            builder.RegisterWebApiModelBinderProvider();
            builder.RegisterType<TestModelBinder>().As<IModelBinder>();
            var container = builder.Build();

            var modelBinders = container.Resolve<IEnumerable<IModelBinder>>().ToList();
            Assert.That(modelBinders.Count(), Is.EqualTo(1));
            Assert.That(modelBinders.First(), Is.InstanceOf<TestModelBinder>());

            var provider = container.Resolve<ModelBinderProvider>();

            Assert.That(provider.GetBinder(new HttpConfiguration(), typeof(TestModel1)), Is.Null);
        }

        static IContainer BuildContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<Dependency>().AsSelf();
            builder.RegisterWebApiModelBinderProvider();
            builder.RegisterType<TestModelBinder>().AsModelBinderForTypes(typeof(TestModel1));

            return builder.Build();
        }
    }
}