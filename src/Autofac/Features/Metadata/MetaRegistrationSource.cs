// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;

namespace Autofac.Features.Metadata
{
    /// <summary>
    /// Support the <see cref="Meta{T}"/>
    /// types automatically whenever type T is registered with the container.
    /// Metadata values come from the component registration's metadata.
    /// </summary>
    internal class MetaRegistrationSource : ImplicitRegistrationSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetaRegistrationSource"/> class.
        /// </summary>
        public MetaRegistrationSource()
            : base(typeof(Meta<>))
        {
        }

        /// <inheritdoc/>
        public override string Description => MetaRegistrationSourceResources.MetaRegistrationSourceDescription;

        /// <inheritdoc/>
        protected override object ResolveInstance<T>(IComponentContext ctx, ResolveRequest request)
            => new Meta<T>((T)ctx.ResolveComponent(request), request.Registration.Target.Metadata);
    }
}
