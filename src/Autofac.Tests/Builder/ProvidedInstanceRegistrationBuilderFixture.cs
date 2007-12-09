using System;
using Autofac.Builder;
using NUnit.Framework;

namespace Autofac.Tests.Builder
{
    [TestFixture]
    public class ProvidedInstanceRegistrationBuilderFixture
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterInstanceNull()
        {
            var builder = new ContainerBuilder();
            builder.Register((object)null);
        }
    }
}
