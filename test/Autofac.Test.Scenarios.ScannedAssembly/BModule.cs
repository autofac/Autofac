﻿// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Test.Scenarios.ScannedAssembly
{
    public class BModule : ModuleBase
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => new BComponent()).As<BComponent>();
        }
    }
}
