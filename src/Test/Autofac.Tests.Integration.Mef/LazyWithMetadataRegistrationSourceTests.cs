using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Integration.Mef;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Mef
{
    public interface IMeta
    {
        int TheInt { get; }
    }

    [TestFixture]
    public class LazyWithMetadataRegistrationSourceTests
    {
        [Test]
        public void WhenGeneratingMetadata_ValuesProvidedFromExtendedProperties()
        {
            var builder = new ContainerBuilder();
            builder.RegisterSource(new LazyWithMetadataRegistrationSource());
            builder.RegisterType<object>().WithExtendedProperty("TheInt", 42);
            var container = builder.Build();
            var lazy = container.Resolve<Lazy<object, IMeta>>();
            Assert.AreEqual(42, lazy.Metadata.TheInt);
        }
    }
}
