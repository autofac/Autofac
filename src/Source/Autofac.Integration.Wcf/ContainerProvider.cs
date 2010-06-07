// This software is part of the Autofac IoC container
// Copyright (c) 2010 Autofac Contributors
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
using System.Linq;
using System.ServiceModel;
using Autofac;

namespace Autofac.Integration.Wcf
{
    /// <summary>
    /// Base class for container providers used in WCF service hosting.
    /// </summary>
    public class ContainerProvider : IContainerProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerProvider"/> class.
        /// </summary>
        /// <param name="applicationContainer">The application container.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="applicationContainer" /> is <see langword="null" />.
        /// </exception>
        public ContainerProvider(IContainer applicationContainer)
        {
            if (applicationContainer == null)
            {
                throw new ArgumentNullException("applicationContainer");
            }
            this.ApplicationContainer = applicationContainer;
        }

        /// <summary>
        /// Gets or sets the application container.
        /// </summary>
        /// <value>
        /// The application container from which request lifetimes will be spawned.
        /// </value>
        public virtual IContainer ApplicationContainer { get; private set; }

        /// <summary>
        /// Disposes of the request lifetime and removes it from the context.
        /// </summary>
        public virtual void EndRequestLifetime()
        {
            var context = OperationContext.Current;
            var extensions = context.Extensions.OfType<AutofacRequestLifetimeScopeExtension>();
            foreach (var extension in extensions)
            {
                extension.RequestLifetime.Dispose();
                context.Extensions.Remove(extension);
            }
        }

        /// <summary>
        /// Gets the request lifetime from the operation context.
        /// </summary>
        /// <value>
        /// An <see cref="Autofac.ILifetimeScope"/> from which individual requests
        /// can be resolved.
        /// </value>
        /// <remarks>
        /// <para>
        /// This property uses the current <see cref="System.ServiceModel.OperationContext"/>
        /// and stores the request lifetime in an <see cref="Autofac.Integration.Wcf.AutofacRequestLifetimeScopeExtension"/>.
        /// If there is no request lifetime found in the context, one is created.
        /// </para>
        /// </remarks>
        public virtual ILifetimeScope RequestLifetime
        {
            get
            {
                var context = OperationContext.Current;
                var extension = context.Extensions.OfType<AutofacRequestLifetimeScopeExtension>().FirstOrDefault();
                if (extension == null)
                {
                    extension = new AutofacRequestLifetimeScopeExtension(this.CreateRequestLifetime());
                    context.Extensions.Add(extension);
                }
                return extension.RequestLifetime;
            }
        }

        /// <summary>
        /// Factory method for creating the request lifetime scope.
        /// </summary>
        /// <returns>
        /// An <see cref="Autofac.ILifetimeScope"/> for resolving request-level dependencies.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method is used by the <see cref="Autofac.Integration.Wcf.ContainerProvider.RequestLifetime"/>
        /// to generate the request lifetime from the application container.
        /// </para>
        /// <para>
        /// This implementation returns a lifetime scope nested in the application container.
        /// </para>
        /// </remarks>
        protected virtual ILifetimeScope CreateRequestLifetime()
        {
            return this.ApplicationContainer.BeginLifetimeScope();
        }
    }
}
