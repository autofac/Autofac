﻿// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;
using Autofac.Test.Scenarios.RegistrationSources;

namespace Autofac.Test;

public sealed class SourceRegistrationExtensionsTests
{
    [Fact]
    public void RegisterSource()
    {
        var source = new EmptyRegistrationSource();
        var builder = new ContainerBuilder();

        builder.RegisterSource(source);

        Assert.False(source.RegistrationsForCalled);
        using (var container = builder.Build())
        {
            Assert.True(source.RegistrationsForCalled);
        }
    }

    [Fact]
    public void RegisterObjectSource()
    {
        var builder = new ContainerBuilder();

        builder.RegisterSource<ObjectRegistrationSource>();

        using (var container = builder.Build())
        {
            var service = container.Resolve<object>();
            Assert.NotNull(service);
        }
    }

    private sealed class EmptyRegistrationSource : IRegistrationSource
    {
        public bool RegistrationsForCalled { get; private set; }

        public bool IsAdapterForIndividualComponents => false;

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
        {
            RegistrationsForCalled = true;
            return Enumerable.Empty<IComponentRegistration>();
        }
    }
}
