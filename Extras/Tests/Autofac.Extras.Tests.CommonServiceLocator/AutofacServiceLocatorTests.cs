using System;
using Autofac;
using Autofac.Extras.CommonServiceLocator;
using Autofac.Extras.Tests.CommonServiceLocator.Components;
using CommonServiceLocator.AutofacAdapter;

using Microsoft.Practices.ServiceLocation;
using NUnit.Framework;

namespace Autofac.Extras.Tests.CommonServiceLocator
{
    [TestFixture]
    public sealed class AutofacServiceLocatorTests : ServiceLocatorTestCase
    {
        protected override IServiceLocator CreateServiceLocator()
        {
            return new AutofacServiceLocator(CreateContainer());
        }

        static IComponentContext CreateContainer()
        {
            var builder = new ContainerBuilder();

            builder
                .RegisterType<SimpleLogger>()
                .Named<ILogger>(typeof (SimpleLogger).FullName)
                .SingleInstance()
                .As<ILogger>();

            builder
                .RegisterType<AdvancedLogger>()
                .Named<ILogger>(typeof (AdvancedLogger).FullName)
                .SingleInstance()
                .As<ILogger>();

            return builder.Build();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_Does_Not_Accept_Null()
        {
            new AutofacServiceLocator(null);
        }
    }
}