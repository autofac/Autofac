// This software is part of the Autofac IoC container
// Copyright (c) 2010 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A1 PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Autofac.Integration.Wcf;
using NUnit.Framework;

namespace UnitTests.Autofac.Integration.Wcf
{
    [TestFixture]
    public class AutofacDependencyInjectionServiceBehaviorFixture
    {
        [Test(Description = "AddBindingParameters should be a no-op.")]
        public void AddBindingParameters_NoOp()
        {
            var data = this.CreateTestData();
            var behavior = new AutofacDependencyInjectionServiceBehavior(data);
            Assert.DoesNotThrow(() => behavior.AddBindingParameters(null, null, null, null));
        }

        [Test(Description = "Verifies that the instance provider is applied to the endpoints.")]
        public void ApplyDispatchBehavior_InstanceProviderApplied()
        {
            var host = this.CreateHost();
            var data = this.CreateTestData();
            host.Opening += (sender, args) => host.Description.Behaviors.Add(new AutofacDependencyInjectionServiceBehavior(data));
            try
            {
                host.Open();
                bool assertionRun = false;
                foreach (ChannelDispatcherBase cdb in host.ChannelDispatchers)
                {
                    ChannelDispatcher cd = cdb as ChannelDispatcher;
                    if (cd != null)
                    {
                        foreach (EndpointDispatcher ed in cd.Endpoints)
                        {
                            Assert.IsInstanceOf<AutofacInstanceProvider>(ed.DispatchRuntime.InstanceProvider, "The instance provider was not applied to all endpoints.");
                            assertionRun = true;
                        }
                    }
                }
                Assert.IsTrue(assertionRun, "Either there weren't any channel dispatchers or there weren't any endpoints opened on the host. The test never ran.");
            }
            finally
            {
                host.Close();
            }
        }

        [Test(Description = "Attempts to apply the behavior given a null service description.")]
        public void ApplyDispatchBehavior_NullDescription()
        {
            ServiceDescription description = null;
            var host = this.CreateHost();
            var data = this.CreateTestData();
            var behavior = new AutofacDependencyInjectionServiceBehavior(data);
            Assert.Throws<ArgumentNullException>(() => behavior.ApplyDispatchBehavior(description, host));
        }

        [Test(Description = "Attempts to apply the behavior given a null service host.")]
        public void ApplyDispatchBehavior_NullHost()
        {
            var description = this.CreateDescription();
            ServiceHostBase host = null;
            var data = this.CreateTestData();
            var behavior = new AutofacDependencyInjectionServiceBehavior(data);
            Assert.Throws<ArgumentNullException>(() => behavior.ApplyDispatchBehavior(description, host));
        }

        [Test(Description = "Ensures you can't create a behavior instance given null implementation data.")]
        public void Ctor_NullData()
        {
            Assert.Throws<ArgumentNullException>(() => new AutofacDependencyInjectionServiceBehavior(null));
        }

        [Test(Description = "Validate should be a no-op.")]
        public void Validate_NoOp()
        {
            var data = this.CreateTestData();
            var behavior = new AutofacDependencyInjectionServiceBehavior(data);
            Assert.DoesNotThrow(() => behavior.Validate(null, null));
        }

        private ServiceImplementationData CreateTestData()
        {
            return new ServiceImplementationData()
            {
                ConstructorString = typeof(IServiceContract).AssemblyQualifiedName,
                ServiceTypeToHost = typeof(IServiceContract),
                ImplementationResolver = l => new ServiceImplementation()
            };
        }

        // Using net.tcp binding because if you run the tests as a non-admin user
        // and use a web binding to localhost, WCF translates it from
        // http://localhost:portnumber/ to http://+:portnumber/ and you end
        // up with a security exception. The net.tcp binding doesn't do that translation
        // so you can run the tests as non-admin and have them pass.
        private static readonly Binding _binding = new NetTcpBinding();
        private static readonly string _address = "net.tcp://localhost:46444/Foo.svc";

        private ServiceDescription CreateDescription()
        {
            var description = new ServiceDescription();
            description.Endpoints.Add(new ServiceEndpoint(ContractDescription.GetContract(typeof(IServiceContract)), _binding, new EndpointAddress(new Uri(_address))));
            return description;
        }

        private ServiceHostBase CreateHost()
        {
            var host = new ServiceHost(typeof(ServiceImplementation));
            host.AddServiceEndpoint(typeof(IServiceContract), _binding, _address);
            return host;
        }

        [ServiceContract]
        private interface IServiceContract
        {
            [OperationContract]
            void Operation();
        }

        private class ServiceImplementation : IServiceContract
        {
            public void Operation()
            {
            }
        }
    }
}
