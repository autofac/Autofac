﻿// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#if NET5_0_OR_GREATER

namespace Autofac.Test.Core.Activators.Reflection;

public class RecordTests
{
    private record Component(IOtherService Service, IOtherService2 Service2);

    [Fact]
    public void CanResolveARecord()
    {
        var builder = new ContainerBuilder();

        builder.RegisterType<OtherComponent>().As<IOtherService>();
        builder.RegisterType<OtherComponent2>().As<IOtherService2>();

        builder.RegisterType<Component>();

        var container = builder.Build();

        var record = container.Resolve<Component>();

        Assert.IsType<OtherComponent>(record.Service);
    }

    private interface IOtherService
    {
    }

    private class OtherComponent : IOtherService
    {
    }

    private interface IOtherService2
    {
    }

    private class OtherComponent2 : IOtherService2
    {
    }
}

#endif
