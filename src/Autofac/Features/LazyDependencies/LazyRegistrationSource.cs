// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Core;

namespace Autofac.Features.LazyDependencies
{
    /// <summary>
    /// Support the <see cref="System.Lazy{T}"/>
    /// type automatically whenever type T is registered with the container.
    /// When a dependency of a lazy type is used, the instantiation of the underlying
    /// component will be delayed until the Value property is first accessed.
    /// </summary>
    internal class LazyRegistrationSource : ImplicitRegistrationSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LazyRegistrationSource"/> class.
        /// </summary>
        public LazyRegistrationSource()
            : base(typeof(Lazy<>))
        {
        }

        /// <inheritdoc/>
        public override string Description => LazyRegistrationSourceResources.LazyRegistrationSourceDescription;

        /// <inheritdoc/>
        protected override object ResolveInstance<T>(IComponentContext context, ResolveRequest request)
        {
            var capturedContext = context.Resolve<IComponentContext>();
            return new Lazy<T>(() => (T)capturedContext.ResolveComponent(request));
        }
    }
}
