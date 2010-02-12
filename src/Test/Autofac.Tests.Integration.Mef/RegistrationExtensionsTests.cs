using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Integration.Mef;
using NUnit.Framework;
using System.ComponentModel.Composition;
using Autofac.Util;
using System.ComponentModel.Composition.Hosting;
using Autofac.Core.Registration;
using Autofac.Core;

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
        [Export]
        public class CircularA
        {
            [ImportingConstructor]
            public CircularA(CircularB b)
            {
                B = b;
            }

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
        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }

        public bool IsDisposed { get; private set; }
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
            var container = RegisterCircularity(typeof(Lazy.CircularA), typeof(Lazy.CircularB));
            var a = container.Resolve<Lazy.CircularA>();
            Assert.IsNotNull(a);
            Assert.IsNotNull(a.B);
            Assert.AreSame(a, a.B.Value.A.Value);
        }

        [Test]
        public void HandlesLazyMefNonPrerequisiteCircularity2()
        {
            var container = RegisterCircularity(typeof(Lazy.CircularA), typeof(Lazy.CircularB));
            var b = container.Resolve<Lazy.CircularB>();
            Assert.IsNotNull(b);
            Assert.IsNotNull(b.A);
            Assert.AreSame(b, b.A.Value.B.Value);
        }

        [Test]
        [Ignore("MEF does not support this case - Autofac probably shouldn't either")]
        public void HandlesEagerMefNonPrerequisiteCircularity1()
        {
            var container = RegisterCircularity(typeof(Eager.CircularA), typeof(Eager.CircularB));
            var a = container.Resolve<Eager.CircularA>();
            Assert.IsNotNull(a);
            Assert.IsNotNull(a.B);
            Assert.AreSame(a, a.B.A);
            Assert.AreSame(a.B, a.B.A.B);
        }

        [Test]
        [Ignore("MEF does not support this case - Autofac probably shouldn't either")]
        public void HandlesEagerMefNonPrerequisiteCircularity2()
        {
            var container = RegisterCircularity(typeof(Eager.CircularA), typeof(Eager.CircularB));
            var b = container.Resolve<Eager.CircularB>();
            Assert.IsNotNull(b);
            Assert.IsNotNull(b.A);
            Assert.AreSame(b, b.A.B);
            Assert.AreSame(b.A, b.A.B.A);
        }

        private static IContainer RegisterCircularity(params Type[] types)
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
            Assert.IsFalse(instance1.IsDisposed);
            container.Dispose();
            Assert.IsTrue(instance1.IsDisposed);
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
            Assert.IsFalse(instance1.IsDisposed);
            Assert.IsFalse(instance2.IsDisposed);
            container.Dispose();
            Assert.IsTrue(instance1.IsDisposed);
            Assert.IsTrue(instance2.IsDisposed);
        }

        [Test]
        public void RespectsExplicitInterchangeServices()
        {
            var builder = new ContainerBuilder();
            var catalog = new TypeCatalog(typeof(HasMultipleExports));

            var interchangeService1 = new TypedService(typeof(HasMultipleExportsBase));
            var interchangeService2 = new NamedService("b", typeof(HasMultipleExports));
            var nonInterchangeService1 = new TypedService(typeof(HasMultipleExports));
            var nonInterchangeService2 = new NamedService("a", typeof(HasMultipleExports));

            builder.RegisterComposablePartCatalog(catalog,
                interchangeService1,
                interchangeService2);

            var container = builder.Build();

            Assert.IsTrue(container.IsRegistered(interchangeService1));
            Assert.IsTrue(container.IsRegistered(interchangeService2));
            Assert.IsFalse(container.IsRegistered(nonInterchangeService1));
            Assert.IsFalse(container.IsRegistered(nonInterchangeService2));
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
    }
}
