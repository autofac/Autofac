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
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using Autofac;
using Autofac.Integration.Wcf;
using NUnit.Framework;

namespace UnitTests.Autofac.Integration.Wcf
{
    [TestFixture]
    public class ServiceHostBehaviorExtensionsFixture
    {
        [Test(Description = "Attempts to register a behavior type with a null builder.")]
        public void RegisterServiceBehaviorForHostT_NullBuilder()
        {
            ContainerBuilder builder = null;
            Assert.Throws<ArgumentNullException>(() => builder.RegisterServiceBehaviorForHost<TestBehavior1>());
        }

        [Test(Description = "Ensures behaviors are registered in the specific named container.")]
        public void RegisterServiceBehaviorForHostT_RegistersBehaviorInNamedContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterServiceBehaviorForHost<TestBehavior1>();
            var container = builder.Build();
            var behaviors = container.Resolve<IEnumerable<IServiceBehavior>>();
            Assert.AreEqual(0, behaviors.Count(), "Behaviors should be registered in the named collection.");
            behaviors = container.ResolveServiceBehaviorsForHost();
            Assert.AreEqual(1, behaviors.Count(), "The named collection of behaviors was empty.");
        }

        [Test(Description = "Resolves the collection of registered behaviors.")]
        public void ResolveServiceBehaviorsForHost_GetsRegisteredBehaviors()
        {
            var builder = new ContainerBuilder();
            builder.RegisterServiceBehaviorForHost<TestBehavior1>();
            builder.RegisterServiceBehaviorForHost<TestBehavior2>();
            var container = builder.Build();
            var behaviors = container.ResolveServiceBehaviorsForHost();
            Assert.AreEqual(2, behaviors.Count(), "Incorrect number of behaviors resolved.");
            Assert.AreEqual(1, behaviors.Where(isb => isb.GetType() == typeof(TestBehavior1)).Count(), "TestBehavior1 not found.");
            Assert.AreEqual(1, behaviors.Where(isb => isb.GetType() == typeof(TestBehavior2)).Count(), "TestBehavior2 not found.");
        }

        [Test(Description = "Resolves the collection of behaviors when none are registered.")]
        public void ResolveServiceBehaviorsForHost_NoRegisteredBehaviors()
        {
            var builder = new ContainerBuilder();
            var container = builder.Build();
            var behaviors = container.ResolveServiceBehaviorsForHost();
            Assert.AreEqual(0, behaviors.Count(), "There should be no behaviors resolved.");
        }

        [Test(Description = "Attempts to resolves the collection of behaviors from a null container.")]
        public void ResolveServiceBehaviorsForHost_NullContainer()
        {
            IContainer container = null;
            Assert.Throws<ArgumentNullException>(() => container.ResolveServiceBehaviorsForHost());
        }

        private class TestBehavior1 : IServiceBehavior
        {
            public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
            {
                throw new NotImplementedException();
            }

            public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
            {
                throw new NotImplementedException();
            }

            public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
            {
                throw new NotImplementedException();
            }
        }

        private class TestBehavior2 : IServiceBehavior
        {
            public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
            {
                throw new NotImplementedException();
            }

            public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
            {
                throw new NotImplementedException();
            }

            public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
            {
                throw new NotImplementedException();
            }
        }
    }
}
