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
using System.ServiceModel;
using Autofac;

namespace Autofac.Integration.Wcf
{
    /// <summary>
    /// Extension for passing around the request lifetime scope.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This extension is on the <see cref="System.ServiceModel.OperationContext"/>
    /// rather than the <see cref="System.ServiceModel.InstanceContext"/>
    /// because a request lifetime is on a per-operation (per 'request') basis
    /// not a per-service-implementation-instance basis.
    /// </para>
    /// </remarks>
    public class AutofacRequestLifetimeScopeExtension : IExtension<OperationContext>
    {
        /// <summary>
        /// Gets the request lifetime scope.
        /// </summary>
        /// <value>
        /// An <see cref="Autofac.ILifetimeScope"/> with the request lifetime.
        /// </value>
        public virtual ILifetimeScope RequestLifetime { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacRequestLifetimeScopeExtension"/> class.
        /// </summary>
        /// <param name="requestLifetime">The request lifetime.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="requestLifetime" /> is <see langword="null" />.
        /// </exception>
        public AutofacRequestLifetimeScopeExtension(ILifetimeScope requestLifetime)
        {
            if (requestLifetime == null)
            {
                throw new ArgumentNullException("requestLifetime");
            }
            this.RequestLifetime = requestLifetime;
        }

        /// <summary>
        /// Enables an extension object to find out when it has been aggregated.
        /// </summary>
        /// <param name="owner">The extensible object that aggregates this extension.</param>
        public virtual void Attach(OperationContext owner)
        {
        }

        /// <summary>
        /// Enables an object to find out when it is no longer aggregated.
        /// </summary>
        /// <param name="owner">The extensible object that aggregates this extension.</param>
        public virtual void Detach(OperationContext owner)
        {
        }
    }
}
