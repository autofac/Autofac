// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;
using Autofac.Core;

namespace Autofac.Features.OwnedInstances
{
    /// <summary>
    /// Generates registrations for services of type <see cref="Owned{T}"/> whenever the service
    /// T is available.
    /// </summary>
    internal class OwnedInstanceRegistrationSource : ImplicitRegistrationSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OwnedInstanceRegistrationSource"/> class.
        /// </summary>
        public OwnedInstanceRegistrationSource()
            : base(typeof(Owned<>))
        {
        }

        /// <inheritdoc/>
        protected override object ResolveInstance<T>(IComponentContext ctx, ResolveRequest request)
        {
            var lifetime = ctx.Resolve<ILifetimeScope>().BeginLifetimeScope(request.Service);
            try
            {
                var value = (T)lifetime.ResolveComponent(request);
                return new Owned<T>(value, lifetime);
            }
            catch
            {
                lifetime.Dispose();
                throw;
            }
        }

        /// <inheritdoc/>
        protected override IRegistrationBuilder<object, SimpleActivatorData, SingleRegistrationStyle> BuildRegistration(IRegistrationBuilder<object, SimpleActivatorData, SingleRegistrationStyle> registration)
            => registration.ExternallyOwned();

        /// <inheritdoc/>
        public override string Description => OwnedInstanceRegistrationSourceResources.OwnedInstanceRegistrationSourceDescription;
    }
}
