using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Autofac.Integration.Mef;
using Autofac.Util;
using Autofac.Core.Registration;
using Autofac.Core;
using Autofac.Features.Metadata;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Mef
{
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

    namespace Lazy
    {
        [Export]
        public class CircularA
        {
            [ImportingConstructor]
            public CircularA(Lazy<CircularB> b)
            {
                B = b;
            }

            public Lazy<CircularB> B { get; private set; }
        }

        [Export]
        public class CircularB
        {
            [Import]
            public Lazy<CircularA> A { get; set; }
        }
    }

    namespace Eager
    {
        /* Non-lazy circular dependencies in MEF have to be done with
         * properties. Constructor parameters will throw a MEF composition
         * exception. */

        [Export]
        public class CircularA
        {
            [Import]
            public CircularB B { get; private set; }
        }

        [Export]
        public class CircularB
        {
            [Import]
            public CircularA A { get; set; }
        }
    }

    public class HasMultipleExportsBase { }

    [Export("a"),
     Export("b"),
     Export(typeof(HasMultipleExportsBase)),
     Export(typeof(HasMultipleExports))]
    public class HasMultipleExports : HasMultipleExportsBase { }

    public class DisposalTracker : Disposable
    {
        public bool IsDisposedPublic
        {
            get
            {
                return this.IsDisposed;
            }
        }
    }

    [Export(typeof(DisposalTracker))]
    public class HasDefaultCreationPolicy : DisposalTracker
    {
    }

    [PartCreationPolicy(CreationPolicy.Any)]
    [Export(typeof(DisposalTracker))]
    public class HasAnyCreationPolicy : DisposalTracker
    {
    }

    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(DisposalTracker))]
    public class HasSharedCreationPolicy : DisposalTracker
    {
    }

    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Export(typeof(DisposalTracker))]
    public class HasNonSharedCreationPolicy : DisposalTracker
    {
    }

    public interface IRequiredMetadata
    {
        string Key { get; }
    }

    [Export]
    public class RequiresMetadata
    {
        [Import]
        public Lazy<string, IRequiredMetadata> Dependency { get; set; }
    }

    [Export]
    public class RequiresMetadataAllowsDefault
    {
        [Import(AllowDefault = true)]
        public Lazy<string, IRequiredMetadata> Dependency { get; set; }
    }

    [Export]
    public class ImportsMany
    {
        [ImportMany]
        public List<string> Dependencies { get; set; }
    }

    public class HasNoMetadata
    {
        [Export]
        public string Service { get { return "Bar"; } }
    }

    public class HasMetadata
    {
        [Export]
        [ExportMetadata("Key", "Foo")]
        public string Service { get { return "Bar"; } }
    }

    [Export]
    public class HasMissingDependency
    {
        [Import]
        public string Dependency { get; set; }
    }

    namespace GenericExports
    {
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

    [TestFixture]
    public class RegistrationExtensionsTests
    {
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
        public void HandlesLazyMefNonPrerequisiteCircularity1()
        {
            var container = RegisterTypeCatalogContaining(typeof(Lazy.CircularA), typeof(Lazy.CircularB));
            var a = container.Resolve<Lazy.CircularA>();
            Assert.IsNotNull(a);
            Assert.IsNotNull(a.B);
            Assert.AreSame(a, a.B.Value.A.Value);
        }

        [Test]
        public void HandlesLazyMefNonPrerequisiteCircularity2()
        {
            var container = RegisterTypeCatalogContaining(typeof(Lazy.CircularA), typeof(Lazy.CircularB));
            var b = container.Resolve<Lazy.CircularB>();
            Assert.IsNotNull(b);
            Assert.IsNotNull(b.A);
            Assert.AreSame(b, b.A.Value.B.Value);
        }

        [Test]
        public void HandlesEagerMefNonPrerequisiteCircularity1()
        {
            var container = RegisterTypeCatalogContaining(typeof(Eager.CircularA), typeof(Eager.CircularB));
            var a = container.Resolve<Eager.CircularA>();
            Assert.IsNotNull(a);
            Assert.IsNotNull(a.B);
            Assert.AreSame(a, a.B.A);
            Assert.AreSame(a.B, a.B.A.B);
        }

        [Test]
        public void HandlesEagerMefNonPrerequisiteCircularity2()
        {
            var container = RegisterTypeCatalogContaining(typeof(Eager.CircularA), typeof(Eager.CircularB));
            var b = container.Resolve<Eager.CircularB>();
            Assert.IsNotNull(b);
            Assert.IsNotNull(b.A);
            Assert.AreSame(b, b.A.B);
            Assert.AreSame(b.A, b.A.B.A);
        }

        [Test(Description = "Issue 326: Generic interfaces should be supported.")]
        public void RegisterComposablePartCatalog_GenericExport()
        {
            var container = RegisterTypeCatalogContaining(typeof(GenericExports.IT1), typeof(GenericExports.ITest<>), typeof(GenericExports.Test));
            var b = container.Resolve<GenericExports.ITest<GenericExports.IT1>>();
            Assert.IsNotNull(b);
            Assert.IsInstanceOf<GenericExports.Test>(b);
        }

        private static IContainer RegisterTypeCatalogContaining(params Type[] types)
        {
            var builder = new ContainerBuilder();
            var catalog = new TypeCatalog(types);
            builder.RegisterComposablePartCatalog(catalog);
            var container = builder.Build();
            return container;
        }

        [Test]
        public void ExcludesExportsWithoutRequiredMetadata()
        {
            var builder = new ContainerBuilder();
            var catalog = new TypeCatalog(typeof(RequiresMetadataAllowsDefault), typeof(HasNoMetadata));
            builder.RegisterComposablePartCatalog(catalog);
            var container = builder.Build();
            var rm = container.Resolve<RequiresMetadataAllowsDefault>();
            Assert.IsNull(rm.Dependency);
        }

        [Test]
        public void IncludesExportsWithRequiredMetadata()
        {
            var builder = new ContainerBuilder();
            builder.RegisterMetadataRegistrationSources();
            var catalog = new TypeCatalog(typeof(RequiresMetadata), typeof(HasMetadata));
            builder.RegisterComposablePartCatalog(catalog);
            var container = builder.Build();
            var rm = container.Resolve<RequiresMetadata>();
            Assert.IsNotNull(rm.Dependency);
        }

        [Test]
        public void SupportsMetadataOnAutofacExports()
        {
            var builder = new ContainerBuilder();
            builder.RegisterMetadataRegistrationSources();
            var metadata = new Dictionary<string, object>
            {
                { "Key", "Value" }
            };
            const string exportedString = "Hello";
            builder.RegisterInstance(exportedString).Exported(e => e.As<string>().WithMetadata(metadata));
            var catalog = new TypeCatalog(typeof(RequiresMetadata));
            builder.RegisterComposablePartCatalog(catalog);
            var container = builder.Build();
            var rm = container.Resolve<RequiresMetadata>();
            Assert.IsNotNull(rm.Dependency);
            Assert.AreEqual(rm.Dependency.Value, "Hello");
        }

        [Test]
        public void SetsMultipleExportsToZeroOrMoreCardinalityImports()
        {
            var builder = new ContainerBuilder();
            var catalog = new TypeCatalog(
                typeof(ImportsMany), typeof(HasMetadata), typeof(HasNoMetadata));
            builder.RegisterComposablePartCatalog(catalog);
            var container = builder.Build();
            var rm = container.Resolve<ImportsMany>();
            Assert.IsNotNull(rm.Dependencies);
            Assert.AreEqual(2, rm.Dependencies.Count());
        }

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
        public void DefaultLifetimeForMefComponentsIsSingleton()
        {
            var builder = new ContainerBuilder();
            var catalog = new TypeCatalog(typeof(HasDefaultCreationPolicy));
            builder.RegisterComposablePartCatalog(catalog);
            AssertDisposalTrackerIsSingleton(builder);
        }

        [Test]
        public void RespectsSharedCreationPolicy()
        {
            var builder = new ContainerBuilder();
            var catalog = new TypeCatalog(typeof(HasSharedCreationPolicy));
            builder.RegisterComposablePartCatalog(catalog);
            AssertDisposalTrackerIsSingleton(builder);
        }

        [Test]
        public void AnyCreationPolicyDefaultsToShared()
        {
            var builder = new ContainerBuilder();
            var catalog = new TypeCatalog(typeof(HasAnyCreationPolicy));
            builder.RegisterComposablePartCatalog(catalog);
            AssertDisposalTrackerIsSingleton(builder);
        }

        private static void AssertDisposalTrackerIsSingleton(ContainerBuilder builder)
        {
            var container = builder.Build();
            var instance1 = container.Resolve<DisposalTracker>();
            var instance2 = container.Resolve<DisposalTracker>();
            Assert.AreSame(instance1, instance2);
            Assert.IsFalse(instance1.IsDisposedPublic);
            container.Dispose();
            Assert.IsTrue(instance1.IsDisposedPublic);
        }

        [Test]
        public void RespectsNonSharedCreationPolicy()
        {
            var builder = new ContainerBuilder();
            var catalog = new TypeCatalog(typeof(HasNonSharedCreationPolicy));
            builder.RegisterComposablePartCatalog(catalog);
            var container = builder.Build();
            var instance1 = container.Resolve<DisposalTracker>();
            var instance2 = container.Resolve<DisposalTracker>();
            Assert.IsAssignableFrom<HasNonSharedCreationPolicy>(instance1);
            Assert.AreNotSame(instance1, instance2);
            Assert.IsFalse(instance1.IsDisposedPublic);
            Assert.IsFalse(instance2.IsDisposedPublic);
            container.Dispose();
            Assert.IsTrue(instance1.IsDisposedPublic);
            Assert.IsTrue(instance2.IsDisposedPublic);
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

        [Test]
        public void RegisterMetadataRegistrationSources_WhenContainerBuilt_AddsStronglyTypedMetaRegistrationSource()
        {
            var builder = new ContainerBuilder();
            builder.RegisterMetadataRegistrationSources();
            var container = builder.Build();

            var stronglyTypedMetaCount = container.ComponentRegistry.Sources
                .Count(source => source is StronglyTypedMetaRegistrationSource);

            Assert.That(stronglyTypedMetaCount, Is.EqualTo(1));
        }

        [Test]
        public void RegisterMetadataRegistrationSources_WhenContainerBuilt_AddsLazyWithMetadataRegistrationSource()
        {
            var builder = new ContainerBuilder();
            builder.RegisterMetadataRegistrationSources();
            var container = builder.Build();

            var lazyWithMetadataCount = container.ComponentRegistry.Sources.Count(
                source => source is LazyWithMetadataRegistrationSource);

            Assert.That(lazyWithMetadataCount, Is.EqualTo(1));
        }

        [Test]
        public void WithMetadata_InterfaceBasedMetadata_SupportLazyWithMetadata()
        {
            var builder = new ContainerBuilder();
            builder.RegisterMetadataRegistrationSources();
            builder.Register(c => new object()).WithMetadata<IMeta>(m =>
                m.For(value => value.TheInt, 42));
            var container = builder.Build();

            var lazy = container.Resolve<Lazy<object, IMeta>>();

            Assert.That(lazy.Metadata.TheInt, Is.EqualTo(42));
            Assert.That(lazy.Value, Is.Not.Null);
        }

        [Test]
        public void WithMetadata_InterfaceBasedMetadata_SupportMeta()
        {
            var builder = new ContainerBuilder();
            builder.RegisterMetadataRegistrationSources();
            builder.Register(c => new object()).WithMetadata<IMeta>(m =>
                m.For(value => value.TheInt, 42));
            var container = builder.Build();

            var meta = container.Resolve<Meta<object, IMeta>>();

            Assert.That(meta.Metadata.TheInt, Is.EqualTo(42));
            Assert.That(meta.Value, Is.Not.Null);
        }
    }
}
