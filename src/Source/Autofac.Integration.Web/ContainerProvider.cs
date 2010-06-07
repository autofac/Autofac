// Contributed by Nicholas Blumhardt 2008-01-28
// Copyright (c) 2010 Autofac Contributors
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
using System.Web;

namespace Autofac.Integration.Web
{
    /// <summary>
    /// Provides application-wide and per-request containers.
    /// </summary>
    public class ContainerProvider : IContainerProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerProvider"/> class.
        /// </summary>
        /// <param name="applicationContainer">The application container.</param>
        public ContainerProvider(IContainer applicationContainer)
        {
            if (applicationContainer == null) throw new ArgumentNullException("applicationContainer");
            this.ApplicationContainer = applicationContainer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerProvider"/> class.
        /// </summary>
        /// <param name="applicationContainer">The application container.</param>
        /// <param name="requestLifetimeConfiguration">
        /// An action that will be executed when building
        /// the per-request lifetime. The components visible within the request can be
        /// customized here.
        /// </param>
        public ContainerProvider(IContainer applicationContainer, Action<ContainerBuilder> requestLifetimeConfiguration)
            : this(applicationContainer)
        {
            this.RequestLifetimeConfiguration = requestLifetimeConfiguration;
        }

        /// <summary>
        /// Dispose of the current request's container, if it has been
        /// instantiated.
        /// </summary>
        public void EndRequestLifetime()
        {
            var rc = AmbientRequestLifetime;
            if (rc != null)
                rc.Dispose();
        }

        /// <summary>
        /// Gets the application container.
        /// </summary>
        /// <value>
        /// The application container from which request lifetimes will be spawned.
        /// </value>
        public virtual IContainer ApplicationContainer { get; private set; }

        /// <summary>
        /// Gets the configuration for each request lifetime.
        /// </summary>
        /// <value>
        /// An <see cref="System.Action{ContainerBuilder}"/> that will be run
        /// for each request lifetime when it is created. If the value is <see langword="null" />,
        /// then no request lifetime configuration will be applied.
        /// </value>
        public virtual Action<ContainerBuilder> RequestLifetimeConfiguration { get; private set; }

        /// <summary>
        /// Gets the request lifetime from the request context.
        /// </summary>
        /// <value>
        /// An <see cref="Autofac.ILifetimeScope"/> from which individual requests
        /// can be resolved.
        /// </value>
        /// <remarks>
        /// <para>
        /// This property uses the current <see cref="System.Web.HttpContext.Items"/>
        /// collection and stores the request lifetime in as a named item.
        /// If there is no request lifetime found in the context, one is created.
        /// </para>
        /// </remarks>
        public virtual ILifetimeScope RequestLifetime
        {
            get
            {
                var result = AmbientRequestLifetime;
                if (result == null)
                {
                    result = this.CreateRequestLifetime();
                    AmbientRequestLifetime = result;
                }

                return result;
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
        /// This method is used by the <see cref="Autofac.Integration.Web.ContainerProvider.RequestLifetime"/>
        /// to generate the request lifetime from the application container.
        /// </para>
        /// <para>
        /// This implementation returns a lifetime scope nested in the application container.
        /// </para>
        /// </remarks>
        protected virtual ILifetimeScope CreateRequestLifetime()
        {
            return this.RequestLifetimeConfiguration == null ?
                        this.ApplicationContainer.BeginLifetimeScope(WebLifetime.Request) :
                        this.ApplicationContainer.BeginLifetimeScope(WebLifetime.Request, this.RequestLifetimeConfiguration);
        }

        /// <summary>
        /// Gets or sets the ambient request lifetime in the current web context.
        /// </summary>
        /// <value>
        /// An <see cref="Autofac.ILifetimeScope"/> that is stored in the current
        /// <see cref="System.Web.HttpContext.Items"/> collection at a known
        /// location.
        /// </value>
        protected static ILifetimeScope AmbientRequestLifetime
        {
            get
            {
                return (ILifetimeScope)HttpContext.Current.Items[typeof(ILifetimeScope)];
            }
            set
            {
                HttpContext.Current.Items[typeof(ILifetimeScope)] = value;
            }
        }
    }
}
