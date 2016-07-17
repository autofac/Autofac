// This software is part of the Autofac IoC container
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
        private Type _implementer;
        private IConstructorFinder _constructorFinder;
        private IConstructorSelector _constructorSelector;

        private static readonly IConstructorFinder DefaultConstructorFinder = new DefaultConstructorFinder();
        private static readonly IConstructorSelector DefaultConstructorSelector = new MostParametersConstructorSelector();

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectionActivatorData"/> class.
        /// </summary>
        /// <param name="implementer">Type that will be activated.</param>
        public ReflectionActivatorData(Type implementer)
        {
            ImplementationType = implementer;

            _constructorFinder = DefaultConstructorFinder;
            _constructorSelector = DefaultConstructorSelector;
        }

        /// <summary>
        /// Gets or sets the implementation type.
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
        /// Gets or sets the constructor finder for the registration.
        /// </summary>
        public IConstructorFinder ConstructorFinder
        {
            get
            {
                return _constructorFinder;
            }

            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                _constructorFinder = value;
            }
        }

        /// <summary>
        /// Gets or sets the constructor selector for the registration.
        /// </summary>
        public IConstructorSelector ConstructorSelector
        {
            get
            {
                return _constructorSelector;
            }

            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                _constructorSelector = value;
            }
        }

        /// <summary>
        /// Gets the explicitly bound constructor parameters.
        /// </summary>
        public IList<Parameter> ConfiguredParameters { get; } = new List<Parameter>();

        /// <summary>
        /// Gets the explicitly bound properties.
        /// </summary>
        public IList<Parameter> ConfiguredProperties { get; } = new List<Parameter>();
    }
}
