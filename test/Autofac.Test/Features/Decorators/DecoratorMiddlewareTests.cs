// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Core.Resolving.Pipeline;
using Autofac.Features.Decorators;
using Moq;
using Xunit;

namespace Autofac.Test.Features.Decorators
{
    public class DecoratorMiddlewareTests
    {
        [Fact]
        public void ResolveOperationDoesNotImplementIDependencyTrackingResolveOperation_DecoratorMiddlewareStoppedEarly()
        {
            var decoratorService = new DecoratorService(typeof(object));
            var middleware = new DecoratorMiddleware(decoratorService, Mock.Of<IComponentRegistration>());
            var contextMock = new Mock<ResolveRequestContext>();
            var registrationMock = new Mock<IComponentRegistration>();
            contextMock.Setup(context => context.Instance).Returns(new object());
            contextMock.Setup(context => context.Registration).Returns(registrationMock.Object);
            registrationMock.Setup(registration => registration.Options).Returns(RegistrationOptions.None);

            middleware.Execute(contextMock.Object, context => { });

            contextMock.Verify(context => context.Instance, Times.Once);
            contextMock.Verify(context => context.Registration.Options, Times.Once);

            // never got further because IResolveOperation is not of type IDependencyTrackingResolveOperation
            contextMock.Verify(context => context.Service, Times.Never);
        }
    }
}
