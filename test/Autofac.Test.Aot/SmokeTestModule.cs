// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;

/// <summary>
/// Verifies module registration (<c>RegisterModule</c>) under Native AOT.
/// </summary>
internal sealed class SmokeTestModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ModuleService>().AsSelf();
    }
}
