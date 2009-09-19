// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2008 Autofac Contributors
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
using System.Linq;
using Autofac.Util;

namespace Autofac
{
    /// <summary>
    /// Extension methods that ease the extraction of particluar kinds of parameter values from
    /// an enumerable sequence of the Parameter abstract type.
    /// Each method returns the first matching parameter value, or throws an exception if
    /// none is provided.
    /// </summary>
    public static class ParameterExtensions
    {
        /// <summary>
        /// Retrieve a parameter value from a <see cref="NamedParameter"/> instance.
        /// </summary>
        /// <typeparam name="T">The type to which the returned value will be cast.</typeparam>
        /// <param name="parameters">The available parameters to choose from.</param>
        /// <param name="name">The name of the parameter to select.</param>
        /// <returns>The value of the selected parameter.</returns>
        public static T Named<T>(this IEnumerable<Parameter> parameters, string name)
        {
            Enforce.ArgumentNotNull(parameters, "parameters");
            Enforce.ArgumentNotNullOrEmpty(name, "name");

            return ConstantValue<NamedParameter, T>(parameters, c => c.Name == name);
        }

        /// <summary>
        /// Retrieve a parameter value from a <see cref="PositionalParameter"/> instance.
        /// </summary>
        /// <typeparam name="T">The type to which the returned value will be cast.</typeparam>
        /// <param name="parameters">The available parameters to choose from.</param>
        /// <param name="position">The zero-based position of the parameter to select.</param>
        /// <returns>The value of the selected parameter.</returns>
        public static T Positional<T>(this IEnumerable<Parameter> parameters, int position)
        {
            Enforce.ArgumentNotNull(parameters, "parameters");
            if (position < 0) throw new ArgumentOutOfRangeException("position");

            return ConstantValue<PositionalParameter, T>(parameters, c => c.Position == position);
        }

        /// <summary>
        /// Retrieve a parameter value from a <see cref="TypedParameter"/> instance.
        /// </summary>
        /// <typeparam name="T">The type to which the returned value will be cast.</typeparam>
        /// <param name="parameters">The available parameters to choose from.</param>
        /// <returns>The value of the selected parameter.</returns>
        public static T TypedAs<T>(this IEnumerable<Parameter> parameters)
        {
            Enforce.ArgumentNotNull(parameters, "parameters");

            return ConstantValue<TypedParameter, T>(parameters, c => c.Type == typeof(T));
        }

        static TValue ConstantValue<TParameter, TValue>(IEnumerable<Parameter> parameters, Func<TParameter, bool> predicate)
            where TParameter : ConstantParameter
        {
            Enforce.ArgumentNotNull(parameters, "parameters");
            Enforce.ArgumentNotNull(predicate, "predicate");

            return parameters
                .OfType<TParameter>()
                .Where(predicate)
                .Select(p => p.Value)
                .Cast<TValue>()
                .First();
        }
    }
}
