﻿// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core.Registration;

namespace Autofac.Test.Core.Registration;

public class ModuleRegistrarTests
{
    [Fact]
    public void Ctor_RequiresContainerBuilder()
    {
        Assert.Throws<ArgumentNullException>(() => new ModuleRegistrar(null));
    }

    [Fact]
    public void RegisterModule_ChainsModuleRegistrations()
    {
        var builder = new ContainerBuilder();
        var registrar = new ModuleRegistrar(builder);
        registrar.RegisterModule(new ModuleA()).RegisterModule(new ModuleB());
        var container = builder.Build();
        var strings = container.Resolve<IEnumerable<string>>();
        Assert.Contains("foo", strings);
        Assert.Contains("bar", strings);
    }

    [Fact]
    public void RegisterModule_RequiresModule()
    {
        var registrar = new ModuleRegistrar(new ContainerBuilder());
        Assert.Throws<ArgumentNullException>(() => registrar.RegisterModule(null));
    }

    private class ModuleA : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance("foo").As<string>();
        }
    }

    private class ModuleB : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance("bar").As<string>();
        }
    }
}
