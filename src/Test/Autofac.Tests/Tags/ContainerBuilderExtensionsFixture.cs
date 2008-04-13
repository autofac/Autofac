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

            builder.Register(c => { instantiations++; return ""; }).InContext(Tag.Outer)
            	.ContainerScoped();

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

            builder.Register(c => { instantiations++; return ""; })
                .InContext(Tag.Outer)
            	.ContainerScoped();

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

            builder.Register(c => "")
                .InContext(Tag.Middle)
            	.ContainerScoped();
            
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

            builder.Register(c => "")
                .InContext(Tag.Outer)
            	.Named(name)
            	.ContainerScoped();

            var outer = builder.Build();
            outer.TagContext(Tag.Outer);

            var s = (string)outer.Resolve(new NamedService(name));
            Assert.IsNotNull(s);
        }
        
        [Test]
        public void CorrectScopeMaintainsOwnership()
        {
        	var tag = "Tag";
        	var builder = new ContainerBuilder();
        	builder.Register(c => new DisposeTracker())
                .InContext(tag)
        		.ContainerScoped();
        	var container = builder.Build();
        	container.TagContext(tag);
        	var inner = container.CreateInnerContainer();
        	var dt = inner.Resolve<DisposeTracker>();
        	Assert.IsFalse(dt.IsDisposed);
        	inner.Dispose();
        	Assert.IsFalse(dt.IsDisposed);
        	container.Dispose();
        	Assert.IsTrue(dt.IsDisposed);
        }
        
        [Test]
        public void FactorySemanticsCorrect()
        {
        	var tag = "Tag";
        	var builder = new ContainerBuilder();
        	builder.Register(c => new object())
                .InContext(tag)
        		.FactoryScoped();
        	var container = builder.Build();
        	container.TagContext(tag);
        	Assert.AreNotSame(container.Resolve<object>(), container.Resolve<object>());
        }
        
        [Test]
        public void DefaultSingletonSemanticsCorrect()
        {
        	var tag = "Tag";
        	var builder = new ContainerBuilder();
        	builder.Register(c => new object()).InContext(tag);
        	var container = builder.Build();
        	container.TagContext(tag);
        	var inner = container.CreateInnerContainer();
        	Assert.AreSame(container.Resolve<object>(), inner.Resolve<object>());
        }
        
        [Test]
        public void ReflectiveRegistration()
        {
        	var tag = "Tag";
        	var builder = new ContainerBuilder();
        	builder.Register(typeof(object)).InContext(tag);
        	var container = builder.Build();
        	container.TagContext(tag);
        	Assert.IsNotNull(container.Resolve<object>());
        }
        
                
        [Test]
        public void RespectsDefaults()
        {
        	var builder = new ContainerBuilder();
        	builder.SetDefaultOwnership(InstanceOwnership.External);
        	builder.SetDefaultScope(InstanceScope.Factory);
        	builder.Register(typeof(DisposeTracker)).InContext("tag");
        	DisposeTracker dt1, dt2;
        	using (var container = builder.Build())
        	{
        		container.TagContext("tag");
        		dt1 = container.Resolve<DisposeTracker>();
        		dt2 = container.Resolve<DisposeTracker>();
        	}
        	
        	Assert.IsNotNull(dt1);
        	Assert.AreNotSame(dt1, dt2);
        	Assert.IsFalse(dt1.IsDisposed);
        	Assert.IsFalse(dt2.IsDisposed);
        }

        [Test]
        public void CollectionsAreTaggable()
        {
        	var builder = new ContainerBuilder();
        	builder.RegisterCollection<object>()
        		.FactoryScoped()
        		.InContext("tag")
        		.As(typeof(IList<object>));
        	
        	var outer = builder.Build();
        	var inner = outer.CreateInnerContainer();
        	inner.TagContext("tag");
        	
        	var coll = inner.Resolve<IList<object>>();
        	Assert.IsNotNull(coll);
        	
        	bool threw = false;
        	try {
        		outer.Resolve<IList<object>>();
        	} catch (Exception) {
        		threw = true;
        	}
        	
        	Assert.IsTrue(threw);
        }

        [Test]
        public void GenericsAreTaggable()
        {
        	var builder = new ContainerBuilder();
        	builder.RegisterGeneric(typeof(List<>))
        		.FactoryScoped()
        		.InContext("tag")
        		.As(typeof(IList<>));
        	
        	var outer = builder.Build();
        	var inner = outer.CreateInnerContainer();
        	inner.TagContext("tag");
        	
        	var coll = inner.Resolve<IList<object>>();
        	Assert.IsNotNull(coll);
        	
        	bool threw = false;
        	try {
        		outer.Resolve<IList<object>>();
        	} catch (Exception) {
        		threw = true;
        	}
        	
        	Assert.IsTrue(threw);
        }
        
        [Test]
        public void AutomaticsAreTaggable()
        {
        	var builder = new ContainerBuilder();
        	builder.RegisterTypesAssignableTo<IList<object>>()
        		.FactoryScoped()
        		.InContext("tag");
        	
        	var outer = builder.Build();
        	var inner = outer.CreateInnerContainer();
        	inner.TagContext("tag");
        	
        	var coll = inner.Resolve<List<object>>();
        	Assert.IsNotNull(coll);
        	
        	bool threw = false;
        	try {
        		outer.Resolve<List<object>>();
        	} catch (Exception) {
        		threw = true;
        	}
        	
        	Assert.IsTrue(threw);
        }
    }
}
