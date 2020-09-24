// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using Xunit;

namespace Autofac.Test.Core.Registration
{
    public class ComponentRegistrationLifetimeDecoratorTests
    {
        [Fact]
        public void DecoratorCallsDisposeOnInnerInstance()
        {
            var inner = Mocks.GetComponentRegistration();
            var decorator = new ComponentRegistrationLifetimeDecorator(inner, CurrentScopeLifetime.Instance);

            decorator.Dispose();

            Assert.True(inner.IsDisposed);
        }
    }
}
