using Autofac.Extras.AggregateService;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Extras.Tests.AggregateService
{
    [TestFixture]
    public class AggregateServiceGenericsFixture
    {
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            var builder = new ContainerBuilder();
            builder.RegisterAggregateService<IOpenGenericAggregate>();
            builder.RegisterGeneric(typeof(OpenGenericImpl<>))
                .As(typeof(IOpenGeneric<>));

            this._container = builder.Build();
        }

        /// <summary>
        /// Attempts to resolve an open generic by a method call
        /// </summary>
        [Test]
        public void Method_ResolveOpenGeneric()
        {
            var aggregateService = this._container.Resolve<IOpenGenericAggregate>();

            var generic = aggregateService.GetOpenGeneric<object>();
            Assert.That(generic, Is.Not.Null);

            var ungeneric = aggregateService.GetResolvedGeneric();
            Assert.That(ungeneric, Is.Not.Null);
            Assert.That(ungeneric, Is.Not.SameAs(generic));
        }
    }
}
