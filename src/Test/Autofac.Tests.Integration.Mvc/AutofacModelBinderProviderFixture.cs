using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Autofac.Integration.Mvc;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Mvc
{
    [TestFixture]
    public class AutofacModelBinderProviderFixture
    {
        [Test]
        public void ProviderIsRegisteredInHttpRequestScope()
        {
            using (ILifetimeScope httpRequestScope = BuilderContainerAndStartHttpRequestScope())
            {
                Assert.That(httpRequestScope.Resolve<IModelBinderProvider>(), Is.InstanceOf<AutofacModelBinderProvider>());
            }
        }

        [Test]
        public void ModelBinderIsRegistered()
        {
            using (ILifetimeScope httpRequestScope = BuilderContainerAndStartHttpRequestScope())
            {
                IEnumerable<IModelBinder> modelBinders = httpRequestScope.Resolve<IEnumerable<IModelBinder>>();
                Assert.That(modelBinders.Count(), Is.EqualTo(1));
            }
        }

        [Test]
        public void ModelBinderHasDependenciesInjected()
        {
            using (ILifetimeScope httpRequestScope = BuilderContainerAndStartHttpRequestScope())
            {
                ModelBinder modelBinder = httpRequestScope.Resolve<IEnumerable<IModelBinder>>()
                    .OfType<ModelBinder>()
                    .FirstOrDefault();
                Assert.That(modelBinder, Is.Not.Null);
                Assert.That(modelBinder.Dependency, Is.Not.Null);
            }
        }

        [Test]
        public void ProviderFindsMatchingModelBinder()
        {
            using (ILifetimeScope httpRequestScope = BuilderContainerAndStartHttpRequestScope())
            {
                AutofacModelBinderProvider provider = (AutofacModelBinderProvider)httpRequestScope.Resolve<IModelBinderProvider>();
                Assert.That(provider.GetBinder(typeof(Model)), Is.InstanceOf<ModelBinder>());
            }
        }

        static ILifetimeScope BuilderContainerAndStartHttpRequestScope()
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterType<Dependency>().AsSelf();
            builder.RegisterModelBinders(Assembly.GetExecutingAssembly());
            builder.RegisterModelBinderProvider();

            IContainer container = builder.Build();
            return container.BeginLifetimeScope(RequestLifetimeHttpModule.HttpRequestTag);
        }
    }

    public class Dependency
    {
    }

    public class Model
    {
    }

    [ModelBinderType(typeof(Model))]
    public class ModelBinder : IModelBinder
    {
        public Dependency Dependency { get; private set; }

        public ModelBinder(Dependency dependency)
        {
            Dependency = dependency;
        }

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            return "Bound";
        }
    }
}