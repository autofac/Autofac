// This software is part of the Autofac IoC container
// Copyright (c) 2013 Autofac Contributors
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
using NHibernate.Bytecode;

namespace Autofac.Extras.NHibernate.Bytecode
{
    /// <summary>
    /// Interface for instanciate all NHibernate objects.
    /// </summary>
    public class AutofacObjectsFactory : IObjectsFactory
    {
        private readonly IComponentContext _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacObjectsFactory"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public AutofacObjectsFactory(IComponentContext container)
        {
            _container = container;
        }

        /// <summary>
        /// Creates an instance of the specified type.
        /// </summary>
        /// <param name="type">The type of object to create.</param>
        /// <returns>
        /// A reference to the created object.
        /// </returns>
        public object CreateInstance(Type type)
        {
            return _container.ResolveOptional(type) ?? Activator.CreateInstance(type);
        }

        /// <summary>
        /// Creates an instance of the specified type.
        /// </summary>
        /// <param name="type">The type of object to create.</param>
        /// <param name="nonPublic">true if a public or nonpublic default constructor can match; false if only a public default constructor can match.</param>
        /// <returns>
        /// A reference to the created object.
        /// </returns>
        public object CreateInstance(Type type, bool nonPublic)
        {
            return _container.ResolveOptional(type) ?? Activator.CreateInstance(type, nonPublic);
        }

        /// <summary>
        /// Creates an instance of the specified type using the constructor
        /// that best matches the specified parameters.
        /// </summary>
        /// <param name="type">The type of object to create.</param>
        /// <param name="ctorArgs">An array of constructor arguments.</param>
        /// <returns>
        /// A reference to the created object.
        /// </returns>
        public object CreateInstance(Type type, params object[] ctorArgs)
        {
            return _container.ResolveOptional(
                    type,
                    (ctorArgs ?? Enumerable.Empty<object>()).Select((p, i) => new PositionalParameter(i, p))) ??
                Activator.CreateInstance(type, ctorArgs);
        }
    }
}