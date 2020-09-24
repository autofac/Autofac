// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

using Autofac.Core.Activators.Delegate;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core
{
    /// <summary>
    /// The <see cref="ComponentRegistration" /> for resolving the current <see cref="ILifetimeScope"/>.
    /// </summary>
    internal sealed class SelfComponentRegistration : ComponentRegistration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelfComponentRegistration"/> class.
        /// </summary>
        public SelfComponentRegistration()
            : base(
                LifetimeScope.SelfRegistrationId,
                new DelegateActivator(typeof(LifetimeScope), (c, p) => { throw new InvalidOperationException(ContainerResources.SelfRegistrationCannotBeActivated); }),
                CurrentScopeLifetime.Instance,
                InstanceSharing.Shared,
                InstanceOwnership.ExternallyOwned,
                new ResolvePipelineBuilder(PipelineType.Registration),
                new Service[] { new TypedService(typeof(ILifetimeScope)), new TypedService(typeof(IComponentContext)) },
                new Dictionary<string, object?>())
        {
        }
    }
}
