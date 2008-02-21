using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac.Builder;
using Autofac.Tags;

namespace Autofac.Tests.Tags
{
    [TestFixture]
    public class ContainerBuilderExtensionsFixture
    {
        enum Tag { None, Outer, Middle, Inner }

        [Test]
        public void OuterSatisfiesInnerResolutions()
        {
            var builder = new ContainerBuilder();

            int instantiations = 0;

            builder.RegisterInContext(c => { instantiations++; return ""; }, Tag.Outer);

            var outer = builder.Build();
            outer.TagContext(Tag.Outer);

            var middle = outer.CreateInnerContainer();
            middle.TagContext(Tag.Middle);

            var inner = middle.CreateInnerContainer();
            inner.TagContext(Tag.Inner);

            middle.Resolve<string>();
            outer.Resolve<string>();
            inner.Resolve<string>();

            Assert.AreEqual(1, instantiations);
        }

        [Test]
        public void AnonymousInnerContainer()
        {
            var builder = new ContainerBuilder();

            int instantiations = 0;

            builder.RegisterInContext(c => { instantiations++; return ""; }, Tag.Outer);

            var outer = builder.Build();
            outer.TagContext(Tag.Outer);

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

            builder.RegisterInContext(c => "", Tag.Middle);
            
            var outer = builder.Build();
            outer.TagContext(Tag.Outer);

            Assert.IsTrue(outer.IsRegistered<string>());
            outer.Resolve<string>();
        }

        [Test]
        public void TaggedRegistrationsAccessibleThroughNames()
        {
            var name = "Name";

            var builder = new ContainerBuilder();

            builder.RegisterInContext(c => "", Tag.Outer).Named(name);

            var outer = builder.Build();
            outer.TagContext(Tag.Outer);

            var s = (string)outer.Resolve(new NamedService(name));
            Assert.IsNotNull(s);
        }
    }
}
