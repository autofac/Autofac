// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Core.Resolving.Pipeline;
using Autofac.Features.Decorators;

namespace Autofac.Test.Features.Decorators;

public class DecoratorMiddlewareTests
{
    [Fact]
    public void ResolveOperationDoesNotImplementIDependencyTrackingResolveOperation_DecoratorMiddlewareStoppedEarly()
    {
        var decoratorService = new DecoratorService(typeof(object));
        var middleware = new DecoratorMiddleware(decoratorService, Substitute.For<IComponentRegistration>());
        var contextMock = Substitute.For<ResolveRequestContext>();
        var registrationMock = Substitute.For<IComponentRegistration>();
        contextMock.Instance.Returns(new object());
        contextMock.Registration.Returns(registrationMock);
        registrationMock.Options.Returns(RegistrationOptions.None);

        middleware.Execute(contextMock, context => { });

        _ = contextMock.Received(1).Instance;
        _ = contextMock.Received(1).Registration.Options;

        // never got further because IResolveOperation is not of type IDependencyTrackingResolveOperation
        _ = contextMock.DidNotReceive().Service;
    }
}
