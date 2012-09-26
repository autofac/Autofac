using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Tests.AppCert.Testing;

namespace Autofac.Tests.AppCert.Tests
{
    [TestFixture]
    public class ResolveFixture
    {
        [Test]
        public void SimpleResolve()
        {
            var builder = ContainerBuilderFactory.Create();
            builder.RegisterType<SimpleService>().As<ISimpleService>();
            var container = builder.Build();
            var service = container.Resolve<ISimpleService>();
            Assert.AreEqual("Simple", service.ServiceValue, "The service did not resolve properly.");
        }

        private interface ISimpleService
        {
            string ServiceValue { get; }
        }

        private class SimpleService : ISimpleService
        {
            public string ServiceValue
            {
                get
                {
                    return "Simple";
                }
            }
        }
    }
}
