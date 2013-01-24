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
    public class CircularDependencyRegistrationTests
    {
        [Test]
        public void HandlesLazyMefNonPrerequisiteCircularity1()
        {
            var container = RegisterTypeCatalogContaining(typeof(LazyCircularA), typeof(LazyCircularB));
            var a = container.Resolve<LazyCircularA>();
            Assert.IsNotNull(a);
            Assert.IsNotNull(a.B);
            Assert.AreSame(a, a.B.Value.A.Value);
        }

        [Test]
        public void HandlesLazyMefNonPrerequisiteCircularity2()
        {
            var container = RegisterTypeCatalogContaining(typeof(LazyCircularA), typeof(LazyCircularB));
            var b = container.Resolve<LazyCircularB>();
            Assert.IsNotNull(b);
            Assert.IsNotNull(b.A);
            Assert.AreSame(b, b.A.Value.B.Value);
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
        public void HandlesEagerMefNonPrerequisiteCircularity1()
        {
            var container = RegisterTypeCatalogContaining(typeof(EagerCircularA), typeof(EagerCircularB));
            var a = container.Resolve<EagerCircularA>();
            Assert.IsNotNull(a);
            Assert.IsNotNull(a.B);
            Assert.AreSame(a, a.B.A);
            Assert.AreSame(a.B, a.B.A.B);
        }

        [Test]
        public void HandlesEagerMefNonPrerequisiteCircularity2()
        {
            var container = RegisterTypeCatalogContaining(typeof(EagerCircularA), typeof(EagerCircularB));
            var b = container.Resolve<EagerCircularB>();
            Assert.IsNotNull(b);
            Assert.IsNotNull(b.A);
            Assert.AreSame(b, b.A.B);
            Assert.AreSame(b.A, b.A.B.A);
        }

        [Export]
        public class LazyCircularA
        {
            [ImportingConstructor]
            public LazyCircularA(Lazy<LazyCircularB> b)
            {
                B = b;
            }

            public Lazy<LazyCircularB> B { get; private set; }
        }

        [Export]
        public class LazyCircularB
        {
            [Import]
            public Lazy<LazyCircularA> A { get; set; }
        }
        /* Non-lazy circular dependencies in MEF have to be done with
         * properties. Constructor parameters will throw a MEF composition
         * exception. */

        [Export]
        public class EagerCircularA
        {
            [Import]
            public EagerCircularB B { get; private set; }
        }

        [Export]
        public class EagerCircularB
        {
            [Import]
            public EagerCircularA A { get; set; }
        }
    }
}
