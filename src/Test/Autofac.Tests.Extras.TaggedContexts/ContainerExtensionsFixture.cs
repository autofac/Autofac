using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac.Extras.TaggedContexts;

namespace Autofac.Tests.Extras.TaggedContexts
{
    [TestFixture]
    public class ContainerExtensionsFixture
    {
        [Test]
        public void EnableTaggedContexts()
        {
            var rootTag = "Root Tag";
            
            var target = new Container();

            target.EnableTaggedContexts(rootTag);

            var tag = target.Resolve<ContextTag<string>>();

            Assert.IsNotNull(tag);
            Assert.AreEqual(rootTag, tag.Tag);
        }

        [Test]
        public void InnerContainerHasTypeDefaultTag()
        {
            var rootTag = "Root Tag";
            
            var target = new Container();

            target.EnableTaggedContexts(rootTag);

            var inner = target.CreateInnerContainer();

            var tag = inner.Resolve<ContextTag<string>>();

            Assert.IsNotNull(tag);
            Assert.AreEqual(default(string), tag.Tag);
        }

        [Test]
        public void EnableTaggedContextsDefaultInNewContexts()
        {
            var rootTag = "Root Tag";
            var defaultTag = "Default Tag";

            var target = new Container();

            target.EnableTaggedContexts(rootTag, defaultTag);

            var inner = target.CreateInnerContainer();

            var tag = inner.Resolve<ContextTag<string>>();

            Assert.IsNotNull(tag);
            Assert.AreEqual(defaultTag, tag.Tag);
        }

        [Test]
        public void CreateTaggedInnerContainer()
        {
            var rootTag = "Root Tag";
            var innerTag = "Inner Tag";

            var target = new Container();

            target.EnableTaggedContexts(rootTag);

            var inner = target.CreateTaggedInnerContainer(innerTag);

            var tag = inner.Resolve<ContextTag<string>>();

            Assert.IsNotNull(tag);
            Assert.AreEqual(innerTag, tag.Tag);
        }
    }
}
