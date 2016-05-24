﻿using Autofac.Core;

namespace Autofac.Test.Scenarios.ScannedAssembly
{
    public class AModule : ModuleBase
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => new AComponent()).As<AComponent>();
        }

        public override bool Equals(IModule other)
        {
            if (other == null) return false;
            return other.GetType() == GetType();
        }
    }
}