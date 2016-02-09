﻿// This software is part of the Autofac IoC container
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
using Autofac.Core;
using Autofac.Core.Activators.Reflection;

namespace Autofac.Builder
{
    /// <summary>
    /// Builder for reflection-based activators.
    /// </summary>
    public class ReflectionActivatorData
    {
        Type _implementer;
        IConstructorFinder _constructorFinder = new DefaultConstructorFinder();
        IPropertyFinder _propertyFinder = new DefaultPropertyFinder();
        IConstructorSelector _constructorSelector = new MostParametersConstructorSelector();

        /// <summary>
        /// Specify a reflection activator for the given type.
        /// </summary>
        /// <param name="implementer">Type that will be activated.</param>
        public ReflectionActivatorData(Type implementer)
        {
            ImplementationType = implementer;
        }

        /// <summary>
        /// Get the implementation type.
        /// </summary>
        public Type ImplementationType
        {
            get
            {
                return _implementer;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                _implementer = value;
            }
        }

        /// <summary>
        /// The constructor finder for the registration.
        /// </summary>
        public IConstructorFinder ConstructorFinder
        {
            get { return _constructorFinder; }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                _constructorFinder = value;
            }
        }

        /// <summary>
        /// The constructor selector for the registration.
        /// </summary>
        public IConstructorSelector ConstructorSelector
        {
            get { return _constructorSelector; }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                _constructorSelector = value;
            }
        }

        /// <summary>
        /// The property selector for the registration
        /// </summary>
        public IPropertyFinder PropertyFinder
        {
            get { return _propertyFinder; }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                _propertyFinder = value;
            }
        }

        /// <summary>
        /// The explicitly bound constructor parameters.
        /// </summary>
        public IList<Parameter> ConfiguredParameters { get; } = new List<Parameter>();

        /// <summary>
        /// The explicitly bound properties.
        /// </summary>
        public IList<Parameter> ConfiguredProperties { get; } = new List<Parameter>();
    }
}
