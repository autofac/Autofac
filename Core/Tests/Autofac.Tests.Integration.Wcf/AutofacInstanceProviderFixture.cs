using System;
using System.ServiceModel.Channels;
using Autofac;
using Autofac.Integration.Wcf;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Wcf
{
    [TestFixture]
    public class AutofacInstanceProviderFixture
    {
        [Test(Description = "Verifies that you must provide a container from which instances will be resolved.")]
        public void Ctor_RequiresContainer()
        {
            var data = new ServiceImplementationData();
            Assert.Throws<ArgumentNullException>(() => new AutofacInstanceProvider(null, data));
        }

        [Test(Description = "Verifies that you must provide data about the service instance to resolve.")]
        public void Ctor_RequiresServiceImplementationData()
        {
            var container = new ContainerBuilder().Build();
            Assert.Throws<ArgumentNullException>(() => new AutofacInstanceProvider(container, null));
        }

        [Test(Description = "Ensures you have to provide an instance context to get an instance.")]
        public void GetInstance_NullInstanceContext()
        {
            var data = new ServiceImplementationData();
            var container = new ContainerBuilder().Build();
            var provider = new AutofacInstanceProvider(container, data);
            var message = new TestMessage();
            Assert.Throws<ArgumentNullException>(() => provider.GetInstance(null, message));
        }

        [Test(Description = "Ensures you have to provide an instance context to release an instance.")]
        public void ReleaseInstance_NullInstanceContext()
        {
            var data = new ServiceImplementationData();
            var container = new ContainerBuilder().Build();
            var provider = new AutofacInstanceProvider(container, data);
            object instance = new object();
            Assert.Throws<ArgumentNullException>(() => provider.ReleaseInstance(null, instance));
        }

        private class TestMessage : Message
        {
            public override MessageHeaders Headers
            {
                get { throw new NotImplementedException(); }
            }

            protected override void OnWriteBodyContents(System.Xml.XmlDictionaryWriter writer)
            {
                throw new NotImplementedException();
            }

            public override MessageProperties Properties
            {
                get { throw new NotImplementedException(); }
            }

            public override MessageVersion Version
            {
                get { throw new NotImplementedException(); }
            }
        }
    }
}
