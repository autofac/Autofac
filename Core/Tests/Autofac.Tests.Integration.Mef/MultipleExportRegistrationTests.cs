using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using Autofac.Core;
using Autofac.Integration.Mef;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Mef
{
    [TestFixture]
    public class MultipleExportRegistrationTests
    {
        [Test]
        public void ImportsEmptyCollectionIfDependencyMissing()
        {
            var builder = new ContainerBuilder();
            var catalog = new TypeCatalog(typeof(ImportsMany));
            builder.RegisterComposablePartCatalog(catalog);
            var container = builder.Build();
            var im = container.Resolve<ImportsMany>();
            Assert.IsNotNull(im.Dependencies);
            Assert.IsFalse(im.Dependencies.Any());
        }

        [Test]
        public void RespectsExplicitInterchangeServices()
        {
            var builder = new ContainerBuilder();
            var catalog = new TypeCatalog(typeof(HasMultipleExports));

            var interchangeService1 = new TypedService(typeof(HasMultipleExportsBase));
            var interchangeService2 = new KeyedService("b", typeof(HasMultipleExports));
            var nonInterchangeService1 = new TypedService(typeof(HasMultipleExports));
            var nonInterchangeService2 = new KeyedService("a", typeof(HasMultipleExports));

            builder.RegisterComposablePartCatalog(catalog,
                interchangeService1,
                interchangeService2);

            var container = builder.Build();

            Assert.IsTrue(container.IsRegisteredService(interchangeService1));
            Assert.IsTrue(container.IsRegisteredService(interchangeService2));
            Assert.IsFalse(container.IsRegisteredService(nonInterchangeService1));
            Assert.IsFalse(container.IsRegisteredService(nonInterchangeService2));
        }

        public class HasMultipleExportsBase { }

        [Export("a"),
         Export("b"),
         Export(typeof(HasMultipleExportsBase)),
         Export(typeof(HasMultipleExports))]
        public class HasMultipleExports : HasMultipleExportsBase { }

        [Export]
        public class ImportsMany
        {
            [ImportMany]
            public List<string> Dependencies { get; set; }
        }
    }
}
