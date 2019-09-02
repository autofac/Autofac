﻿// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// https://autofac.org
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
using System.Reflection;

namespace Autofac.Core.Activators.Reflection
{
    /// <summary>
    /// Supplies values based on the target parameter type.
    /// </summary>
    public class AutowiringParameter : Parameter
    {
        /// <summary>
        /// Returns true if the parameter is able to provide a value to a particular site.
        /// </summary>
        /// <param name="pi">Constructor, method, or property-mutator parameter.</param>
        /// <param name="context">The component context in which the value is being provided.</param>
        /// <param name="valueProvider">If the result is true, the valueProvider parameter will
        /// be set to a function that will lazily retrieve the parameter value. If the result is false,
        /// will be set to null.</param>
        /// <returns>True if a value can be supplied; otherwise, false.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="pi" /> or <paramref name="context" /> is <see langword="null" />.
        /// </exception>
        public override bool CanSupplyValue(ParameterInfo pi, IComponentContext context, out Func<object> valueProvider)
        {
            if (pi == null) throw new ArgumentNullException(nameof(pi));
            if (context == null) throw new ArgumentNullException(nameof(context));

            var service = new TypedService(pi.ParameterType);
            if (context.ComponentRegistry.TryGetRegistration(service, out var registration))
            {
                valueProvider = () => context.ResolveComponent(new ResolveRequest(service, registration, Enumerable.Empty<Parameter>()));
                return true;
            }

            valueProvider = null;
            return false;
        }
    }
}
