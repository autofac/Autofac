// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;

namespace A;

public class LifetimeScopeEndingModule : Module
{
    private readonly Action _invokeOnEndCallback;

    public LifetimeScopeEndingModule(Action invokeOnEndCallback)
    {
        _invokeOnEndCallback = invokeOnEndCallback;
    }

    protected override void Load(ContainerBuilder builder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.RegisterType<Service1>();

        builder.RegisterBuildCallback(scope => scope.CurrentScopeEnding += (sender, ev) =>
        {
            _invokeOnEndCallback();
        });
    }
}
