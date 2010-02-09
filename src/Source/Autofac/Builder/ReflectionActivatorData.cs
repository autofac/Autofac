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
using System.Collections.Generic;
using System.Reflection;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Autofac.Util;

namespace Autofac.Builder
{
    /// <summary>
    /// Builder for reflection-based activators.
    /// </summary>
    public class ReflectionActivatorData
    {
        Type _implementor;
        IConstructorFinder _constructorFinder = new BindingFlagsConstructorFinder(BindingFlags.Public);
        IConstructorSelector _constructorSelector = new MostParametersConstructorSelector();
        readonly IList<Parameter> _configuredParameters = new List<Parameter>();
        readonly IList<Parameter> _configuredProperties = new List<Parameter>();

        /// <summary>
        /// Specify a reflection activator for the given type.
        /// </summary>
        /// <param name="implementor">Type that will be activated.</param>
        public ReflectionActivatorData(Type implementor)
        {
            ImplementationType = implementor;
        }

        /// <summary>
        /// Get the implementation type.
        /// </summary>
        public Type ImplementationType
        {
            get
            {
                return _implementor;
            }
            set
            {
                _implementor = Enforce.ArgumentNotNull(value, "value");
            }
        }

        /// <summary>
        /// The constructor finder for the registration.
        /// </summary>
        public IConstructorFinder ConstructorFinder
        {
            get { return _constructorFinder; }
            set { _constructorFinder = Enforce.ArgumentNotNull(value, "value"); }
        }

        /// <summary>
        /// The constructor selector for the registration.
        /// </summary>
        public IConstructorSelector ConstructorSelector
        {
            get { return _constructorSelector; }
            set { _constructorSelector = Enforce.ArgumentNotNull(value, "value"); }
        }

        /// <summary>
        /// The explicitly bound constructor parameters.
        /// </summary>
        public IList<Parameter> ConfiguredParameters
        {
            get { return _configuredParameters; }
        }

        /// <summary>
        /// The explicitly bound properties.
        /// </summary>
        public IList<Parameter> ConfiguredProperties
        {
            get { return _configuredProperties; }
        }
    }
}
