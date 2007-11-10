// This software is part of the Autofac IoC container
// Copyright (c) 2007 Nicholas Blumhardt
// nicholas.blumhardt@gmail.com
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
using Autofac.Component.Registration;

namespace Autofac.Builder
{
    /// <summary>
    /// Registers a type as a collection type.
    /// </summary>
	class CollectionRegistrar : IModule
	{
        Type _collectedService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionRegistrar"/> class.
        /// </summary>
        /// <param name="collectedService">The collected service.</param>
        public CollectionRegistrar(Type collectedService)
        {
            Enforce.ArgumentNotNull(collectedService, "collectedService");
            _collectedService = collectedService;
        }

		#region IComponentRegistrar Members

        /// <summary>
        /// Registers the component.
        /// </summary>
        /// <param name="container">The container.</param>
		public void Configure(Container container)
		{
            Enforce.ArgumentNotNull(container, "container");
            Type registrationType = typeof(CollectionRegistration<>).MakeGenericType(_collectedService);
            ICollectionRegistration registration = (ICollectionRegistration)Activator.CreateInstance(registrationType);
			container.RegisterCollection(registration);
		}

		#endregion
	}
}
