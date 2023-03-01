// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;

namespace A;

public class OnActivatedModule : Module
{
    private readonly int _value;

    public OnActivatedModule(int value)
    {
        _value = value;
    }

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<Service1>().OnActivated(x => x.Instance.Value = _value);
    }
}
