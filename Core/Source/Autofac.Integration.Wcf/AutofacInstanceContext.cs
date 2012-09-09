// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.ServiceModel;
using Autofac.Core;
using Autofac.Core.Registration;

namespace Autofac.Integration.Wcf
{
    /// <summary>
    /// Manages instance lifecycle using an Autofac inner container.
    /// </summary>
    public class AutofacInstanceContext : IExtension<InstanceContext>, IDisposable, IComponentContext
    {
        readonly ILifetimeScope _lifetime;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacInstanceContext"/> class.
        /// </summary>
        /// <param name="container">The outer container.</param>
        public AutofacInstanceContext(ILifetimeScope container)
        {
            if (container == null)
                throw new ArgumentNullException("container");

            _lifetime = container.BeginLifetimeScope();
        }

        /// <summary>
        /// Enables an extension object to find out when it has been aggregated.
        /// Called when the extension is added to the
        /// <see cref="P:System.ServiceModel.IExtensibleObject`1.Extensions"/> property.
        /// </summary>
        /// <param name="owner">The extensible object that aggregates this extension.</param>
        public void Attach(InstanceContext owner)
        {
        }

        /// <summary>
        /// Enables an object to find out when it is no longer aggregated.
        /// Called when an extension is removed from the
        /// <see cref="P:System.ServiceModel.IExtensibleObject`1.Extensions"/> property.
        /// </summary>
        /// <param name="owner">The extensible object that aggregates this extension.</param>
        public void Detach(InstanceContext owner)
        {
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or
        /// resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _lifetime.Dispose();
        }

        /// <summary>
        /// Associates services with the components that provide them.
        /// </summary>
        public IComponentRegistry ComponentRegistry
        {
            get { return _lifetime.ComponentRegistry; }
        }

        /// <summary>
        /// Resolve an instance of the provided registration within the context.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <param name="parameters">Parameters for the instance.</param>
        /// <returns>
        /// The component instance.
        /// </returns>
        /// <exception cref="ComponentNotRegisteredException"/>
        /// <exception cref="Autofac.Core.DependencyResolutionException"/>
        public object ResolveComponent(IComponentRegistration registration, IEnumerable<Parameter> parameters)
        {
            return _lifetime.ResolveComponent(registration, parameters);
        }
    }
}
