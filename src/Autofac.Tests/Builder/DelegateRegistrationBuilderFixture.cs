using System;
using Autofac.Builder;
using NUnit.Framework;

namespace Autofac.Tests.Builder
{
    [TestFixture]
    public class DelegateRegistrationBuilderFixture
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterDelegateNull()
        {
            var target = new ContainerBuilder();
            target.Register((ComponentActivator<object>)null);
        }
    }
}
