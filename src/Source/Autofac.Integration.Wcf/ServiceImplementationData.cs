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
using Autofac.Core;

namespace Autofac.Integration.Wcf
{
    /// <summary>
    /// Contains data about a WCF service implementation.
    /// </summary>
    public class ServiceImplementationData
    {
        /// <summary>
        /// Gets or sets the string used to generate the data.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> used to generate this service
        /// implementation data.
        /// </value>
        public string ConstructorString { get; set; }

        /// <summary>
        /// Gets or sets the concrete type to host.
        /// </summary>
        /// <value>
        /// A <see cref="System.Type"/> that indicates the type to pass to the
        /// service host when it is initially created. This type must be a concrete
        /// class and not an interface.
        /// </value>
        public Type ServiceTypeToHost { get; set; }

        /// <summary>
        /// Gets or sets a mechanism that allows the <see cref="AutofacInstanceProvider"/>
        /// to get the actual implementation for a service.
        /// </summary>
        /// <value>
        /// An <see cref="System.Func{T,U}"/> that takes in a lifetime scope returns
        /// an <see cref="System.Object"/> that is the implementation type for the
        /// given service. This is the object that the service host will use
        /// and should be assignable from the <see cref="Autofac.Integration.Wcf.ServiceImplementationData.ServiceTypeToHost"/>.
        /// </value>
        public Func<ILifetimeScope, object> ImplementationResolver { get; set; }
    }
}
