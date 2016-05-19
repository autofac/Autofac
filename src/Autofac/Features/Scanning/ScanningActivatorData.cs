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
using Autofac.Builder;
using Autofac.Core;

namespace Autofac.Features.Scanning
{
    /// <summary>
    /// Activation data for types located by scanning assemblies.
    /// </summary>
    public class ScanningActivatorData : ReflectionActivatorData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScanningActivatorData"/> class.
        /// </summary>
        public ScanningActivatorData()
            : base(typeof(object)) // TODO - refactor common base class out of RAD
        {
        }

        /// <summary>
        /// Gets the filters applied to the types from the scanned assembly.
        /// </summary>
        public ICollection<Func<Type, bool>> Filters { get; } = new List<Func<Type, bool>>();

        /// <summary>
        /// Gets the additional actions to be performed on the concrete type registrations.
        /// </summary>
        public ICollection<Action<Type, IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>>> ConfigurationActions { get; }
            = new List<Action<Type, IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>>>();

        /// <summary>
        /// Gets the actions to be called once the scanning operation is complete.
        /// </summary>
        public ICollection<Action<IComponentRegistry>> PostScanningCallbacks { get; } = new List<Action<IComponentRegistry>>();
    }
}
