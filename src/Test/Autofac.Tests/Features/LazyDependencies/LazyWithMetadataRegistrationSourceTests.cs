using System;
using Autofac.Features.LazyDependencies;
using NUnit.Framework;

namespace Autofac.Tests.Features.LazyDependencies
{
    public interface IMeta
    {
        int TheInt { get; }
    }

    [TestFixture]
    public class LazyWithMetadataRegistrationSourceTests
    {
        [Test]
        public void WhenGeneratingMetadata_ValuesProvidedFromMetadata()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<object>().WithMetadata("TheInt", 42);
            var container = builder.Build();
            var lazy = container.Resolve<Lazy<object, IMeta>>();
            Assert.AreEqual(42, lazy.Metadata.TheInt);
        }
    }
}
