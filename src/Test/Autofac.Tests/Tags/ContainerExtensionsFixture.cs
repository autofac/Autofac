//using System;
//using NUnit.Framework;

//namespace Autofac.Tests.Tags
//{
//    [TestFixture]
//    public class ContainerExtensionsFixture
//    {
//        [Test]
//        public void EnableTaggedContexts()
//        {
//            var rootTag = "Root Tag";
            
//            var target = new Container();

//            target.TagWith(rootTag);

//            var tag = target.Resolve<ContextTag<string>>();

//            Assert.IsNotNull(tag);
//            Assert.AreEqual(rootTag, tag.Tag);
//        }

//        [Test]
//        public void AnonymousContainerHasTypeNoTag()
//        {
//            var rootTag = "Root Tag";
            
//            var target = new Container();

//            target.TagWith(rootTag);

//            var inner = target.BeginLifetimeScope();

//            var tag = inner.Resolve<ContextTag<string>>();

//            Assert.IsFalse(tag.HasTag);
//        }

//        [Test]
//        [ExpectedException(typeof(InvalidOperationException))]
//        public void EnableTaggedContextsDefaultInNewContexts()
//        {
//            var rootTag = "Root Tag";

//            var target = new Container();
//            target.TagWith(rootTag);

//            var inner = target.BeginLifetimeScope();

//            var tag = inner.Resolve<ContextTag<string>>();

//            Assert.IsNotNull(tag);
//            Assert.IsFalse(tag.HasTag);
//            var unused = tag.Tag;
//        }

//        [Test]
//        public void CreateTaggedInnerContainer()
//        {
//            var rootTag = "Root Tag";
//            var innerTag = "Inner Tag";

//            var target = new Container();

//            target.TagWith(rootTag);

//            var inner = target.BeginLifetimeScope();
//            inner.TagWith(innerTag);

//            var tag = inner.Resolve<ContextTag<string>>();

//            Assert.IsNotNull(tag);
//            Assert.AreEqual(innerTag, tag.Tag);
//        }
//    }
//}
