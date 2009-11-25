// Contributed by Nicholas Blumhardt 2008-01-28
// Copyright (c) 2007 - 2009 Autofac Contributors
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
        IContainer _applicationContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerProvider"/> class.
        /// </summary>
        /// <param name="applicationContainer">The application container.</param>
        public ContainerProvider(IContainer applicationContainer)
        {
            if (applicationContainer == null)
                throw new ArgumentNullException("applicationContainer");

            applicationContainer.Tag = WebLifetime.Application;
            _applicationContainer = applicationContainer;
        }

        /// <summary>
        /// Dispose of the current request's container, if it has been
        /// instantiated.
        /// </summary>
        public void EndRequestLifetime()
        {
            var rc = NullableRequestLifetime;
            if (rc != null)
                rc.Dispose();
        }

        /// <summary>
        /// The global, application-wide container.
        /// </summary>
        /// <value></value>
        public IContainer ApplicationContainer
        {
            get
            {
                return _applicationContainer;
            }
        }

        /// <summary>
        /// The container used to manage components for processing the
        /// current request.
        /// </summary>
        /// <value></value>
        public ILifetimeScope RequestLifetime
        {
            get
            {
                var result = NullableRequestLifetime;
                if (result == null)
                {
                    result = NullableRequestLifetime = ApplicationContainer.BeginLifetimeScope();
                    result.Tag = WebLifetime.Request;
                }

                return result;
            }
        }

        ILifetimeScope NullableRequestLifetime
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
