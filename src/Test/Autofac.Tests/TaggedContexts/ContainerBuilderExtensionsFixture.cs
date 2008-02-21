using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac.Builder;
using Autofac.TaggedContexts;

namespace Autofac.Tests.TaggedContexts
{
    [TestFixture]
    public class ContainerBuilderExtensionsFixture
    {
        enum Tag { None, Outer, Middle, Inner }

        [Test]
        public void OuterSatisfiesInnerResolutions()
        {
            var builder = new ContainerBuilder();
            builder.EnableTaggedContexts(Tag.Outer, Tag.None);

            int instantiations = 0;

            builder.RegisterInContext(c => { instantiations++; return ""; }, Tag.Outer);

            var outer = builder.Build();
            var middle = outer.CreateTaggedInnerContainer(Tag.Middle);
            var inner = middle.CreateTaggedInnerContainer(Tag.Inner);

            middle.Resolve<string>();
            outer.Resolve<string>();
            inner.Resolve<string>();

            Assert.AreEqual(1, instantiations);
        }

        [Test]
        public void AnonymousInnerContainer()
        {
            var builder = new ContainerBuilder();
            builder.EnableTaggedContexts(Tag.Outer, Tag.None);

            int instantiations = 0;

            builder.RegisterInContext(c => { instantiations++; return ""; }, Tag.Outer);

            var outer = builder.Build();
            var anon = outer.CreateInnerContainer();

            anon.Resolve<string>();
            outer.Resolve<string>();

            Assert.AreEqual(1, instantiations);
        }

        [Test]
        [ExpectedException(typeof(DependencyResolutionException))]
        public void InnerRegistrationNotAccessibleToOuter()
        {
            var builder = new ContainerBuilder();
            builder.EnableTaggedContexts(Tag.Outer, Tag.None);
            builder.RegisterInContext(c => "", Tag.Middle);
            
            var outer = builder.Build();

            Assert.IsTrue(outer.IsRegistered<string>());
            outer.Resolve<string>();
        }

        [Test]
        public void TaggedRegistrationsAccessibleThroughNames()
        {
            var name = "Name";

            var builder = new ContainerBuilder();
            builder.EnableTaggedContexts(Tag.Outer, Tag.None);
            builder.RegisterInContext(c => "", Tag.Outer).Named(name);

            var outer = builder.Build();

            var s = (string)outer.Resolve(new NamedService(name));
            Assert.IsNotNull(s);
        }
    }
}
