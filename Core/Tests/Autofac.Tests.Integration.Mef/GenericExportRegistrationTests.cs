using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using Autofac.Integration.Mef;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Mef
{
    [TestFixture]
    public class GenericExportRegistrationTests
    {
        [Test(Description = "Issue 326: Generic interfaces should be supported.")]
        public void RegisterComposablePartCatalog_GenericExport()
        {
            var container = RegisterTypeCatalogContaining(typeof(IT1), typeof(ITest<>), typeof(Test));
            var b = container.Resolve<ITest<IT1>>();
            Assert.IsNotNull(b);
            Assert.IsInstanceOf<Test>(b);
        }

        private static IContainer RegisterTypeCatalogContaining(params Type[] types)
        {
            var builder = new ContainerBuilder();
            var catalog = new TypeCatalog(types);
            builder.RegisterComposablePartCatalog(catalog);
            var container = builder.Build();
            return container;
        }

        [InheritedExport]
        public interface ITest<T> { }

        public interface IT1 { }

        public class Test : ITest<IT1> { }

        public class TestConsumer
        {
            [Import]
            public ITest<IT1> Property { get; set; }
        }
    }
}
