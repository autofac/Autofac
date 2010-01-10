using Autofac.Integration.Mef;
using NUnit.Framework;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;

namespace Autofac.Tests.Integration.Mef
{
    /// <summary>
    /// See Autofac Issue 128.
    /// Courtesy of palpatine@kopernet.org
    /// </summary>
    [TestFixture]
    public class LifetimeScenariosTests
    {
        public class RegisteredInAutofac2
        {
            public ExportedToMefAndImportingFromAutofac ImportedFormMef { get; set; }
            public RegisteredInAutofac2(ExportedToMefAndImportingFromAutofac importedFormMef)
            {
                ImportedFormMef = importedFormMef;
            }
        }

        [Export]
        [PartCreationPolicy(CreationPolicy.NonShared)]
        public class ExportedToMefAndImportingFromAutofac
        {
            [Import]
            public RegisteredInAutofacAndExported ImportedFormAutofac { get; set; }
        }

        public class RegisteredInAutofacAndExported
        {
        }

        [Test]
        public void IsClassRegistredInAutofacAsFactoryScopedResolvedByMefAsFactoryScoped()
        {
            var containerBuilder = new ContainerBuilder();

            var newAssemblyCatalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
            containerBuilder.RegisterComposablePartCatalog(newAssemblyCatalog);
            containerBuilder.RegisterType<RegisteredInAutofac2>();
            containerBuilder.RegisterType<RegisteredInAutofacAndExported>()
                .Exported(e => e.As<RegisteredInAutofacAndExported>());

            var autofacContainer = containerBuilder.Build();

            var elementFromAutofac1 = autofacContainer.Resolve<RegisteredInAutofac2>();
            var elementFromAutofac2 = autofacContainer.Resolve<RegisteredInAutofac2>();

            Assert.AreNotSame(elementFromAutofac1, elementFromAutofac2);
            Assert.AreNotSame(elementFromAutofac1.ImportedFormMef, elementFromAutofac2.ImportedFormMef);
            Assert.AreNotSame(elementFromAutofac1.ImportedFormMef.ImportedFormAutofac, elementFromAutofac2.ImportedFormMef.ImportedFormAutofac);//fail
        }
    }
}
