using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using Autofac.Core.Registration;
using Autofac.Integration.Mef;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Mef
{
    [TestFixture]
    public class SimpleRegistrationTests
    {
        [Test]
        public void MissingDependencyDetected()
        {
            var builder = new ContainerBuilder();
            var catalog = new TypeCatalog(typeof(HasMissingDependency));
            builder.RegisterComposablePartCatalog(catalog);
            var container = builder.Build();
            Assert.Throws<ComponentNotRegisteredException>(() => container.Resolve<HasMissingDependency>());
        }

        [Test]
        public void RetrievesExportedInterfaceFromCatalogPart()
        {
            var builder = new ContainerBuilder();
            var catalog = new TypeCatalog(typeof(Foo));
            builder.RegisterComposablePartCatalog(catalog);
            var container = builder.Build();
            var foo = container.Resolve<IFoo>();
            Assert.IsAssignableFrom<Foo>(foo);
        }

        [Test]
        public void SatisfiesImportOnMefComponentFromAutofac()
        {
            var builder = new ContainerBuilder();
            var catalog = new TypeCatalog(typeof(Bar));
            builder.RegisterComposablePartCatalog(catalog);
            builder.RegisterType<Foo>().Exported(e => e.As<IFoo>());
            var container = builder.Build();
            var bar = container.Resolve<Bar>();
            Assert.IsNotNull(bar.Foo);
        }

        [Test]
        public void SatisfiesImportOnMefComponentFromMef()
        {
            var builder = new ContainerBuilder();
            var catalog = new TypeCatalog(typeof(Foo), typeof(Bar));
            builder.RegisterComposablePartCatalog(catalog);
            var container = builder.Build();
            var bar = container.Resolve<Bar>();
            Assert.IsNotNull(bar.Foo);
        }

        [Test]
        public void ResolvesExportsFromContext()
        {
            var builder = new ContainerBuilder();
            var catalog = new TypeCatalog(typeof(Foo));
            builder.RegisterComposablePartCatalog(catalog);
            builder.RegisterType<Foo>().Exported(e => e.As<IFoo>());
            var container = builder.Build();
            var exports = container.ResolveExports<IFoo>();
            Assert.AreEqual(2, exports.Count());
        }

        [Test]
        public void RestrictsExportsBasedOnValueType()
        {
            var builder = new ContainerBuilder();
            const string n = "name";
            builder.RegisterType<Foo>().Exported(e => e.AsNamed<IFoo>(n));
            builder.RegisterType<Foo>().Exported(e => e.AsNamed<Foo>(n));
            var container = builder.Build();
            var exports = container.ResolveExports<IFoo>(n);
            Assert.AreEqual(1, exports.Count());
        }

        public interface IFoo { }

        [Export(typeof(IFoo))]
        public class Foo : IFoo
        {
        }

        [Export]
        public class Bar
        {
            [ImportingConstructor]
            public Bar(IFoo foo)
            {
                Foo = foo;
            }

            public IFoo Foo { get; private set; }
        }

        [Export]
        public class HasMissingDependency
        {
            [Import]
            public string Dependency { get; set; }
        }

        public class ObjectExportBaseClass { }

        [Export("contract-name", typeof(object))]
        public class ObjectExportDerivedClass : ObjectExportBaseClass { }

        public class ObjectExportImporter
        {
            [Import("contract-name")]
            public object Item { get; set; }
        }
    }
}
