// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Core;
using Xunit;

namespace Autofac.Test.Core
{
    public class ComponentRegisteredEventArgsTests
    {
        [Fact]
        public void ConstructorSetsProperties()
        {
            var registry = Factory.CreateEmptyComponentRegistryBuilder();
            var registration = Factory.CreateSingletonObjectRegistration();
            var args = new ComponentRegisteredEventArgs(registry, registration);
            Assert.Same(registry, args.ComponentRegistryBuilder);
            Assert.Same(registration, args.ComponentRegistration);
        }

        [Fact]
        public void NullContainerDetected()
        {
            var registration = Factory.CreateSingletonObjectRegistration();
            Assert.Throws<ArgumentNullException>(() => new ComponentRegisteredEventArgs(null, registration));
        }

        [Fact]
        public void NullRegistrationDetected()
        {
            var registry = Factory.CreateEmptyComponentRegistryBuilder();
            Assert.Throws<ArgumentNullException>(() => new ComponentRegisteredEventArgs(registry, null));
        }
    }
}
