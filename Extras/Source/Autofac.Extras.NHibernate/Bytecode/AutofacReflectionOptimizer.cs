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
using NHibernate.Bytecode.Lightweight;
using NHibernate.Properties;

namespace Autofac.Extras.NHibernate.Bytecode
{
    /// <summary>
    /// Reflection optimizer implementation.
    /// </summary>
    public class AutofacReflectionOptimizer : ReflectionOptimizer
    {
        private readonly IComponentContext _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacReflectionOptimizer"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="mappedType">The type being mapped.</param>
        /// <param name="getters">The getters.</param>
        /// <param name="setters">The setters.</param>
        public AutofacReflectionOptimizer(IComponentContext container, Type mappedType, IGetter[] getters, ISetter[] setters)
            : base(mappedType, getters, setters)
        {
            _container = container;
        }

        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <returns>The instance.</returns>
        public override object CreateInstance()
        {
            if (_container.IsRegistered(mappedType))
                return _container.Resolve(mappedType);
            
            return _container.IsRegisteredWithName(mappedType.FullName, mappedType)
                       ? _container.ResolveNamed(mappedType.FullName, mappedType)
                       : base.CreateInstance();
        }

        /// <summary>
        /// Determines if an exception should be thrown for when no default constructor is found.
        /// </summary>
        /// <param name="type">The type.</param>
        protected override void ThrowExceptionForNoDefaultCtor(Type type) { }
    }
}