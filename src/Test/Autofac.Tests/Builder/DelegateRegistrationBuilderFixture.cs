using System;
using Autofac.Builder;
using NUnit.Framework;
using Autofac.Core;

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
            target.RegisterDelegate((Func<IComponentContext, object>)null);
        }

        [Test]
        public void ExposesImplementationType()
        {
            var cb = new ContainerBuilder();
            cb.RegisterDelegate(c => "Hello").As<object>();
            var container = cb.Build();
            IComponentRegistration cr;
            Assert.IsTrue(container.ComponentRegistry.TryGetRegistration(
                new TypedService(typeof(object)), out cr));
            Assert.AreEqual(typeof(string), cr.Activator.LimitType);
        }
    }
}
