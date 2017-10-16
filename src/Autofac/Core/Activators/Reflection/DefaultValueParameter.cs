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
using System.Reflection;

namespace Autofac.Core.Activators.Reflection
{
    /// <summary>
    /// Provides parameters that have a default value, set with an optional parameter
    /// declaration in C# or VB.
    /// </summary>
    public class DefaultValueParameter : Parameter
    {
        /// <summary>
        /// Returns true if the parameter is able to provide a value to a particular site.
        /// </summary>
        /// <param name="pi">Constructor, method, or property-mutator parameter.</param>
        /// <param name="context">The component context in which the value is being provided.</param>
        /// <param name="valueProvider">If the result is true, the <paramref name="valueProvider" /> parameter will
        /// be set to a function that will lazily retrieve the parameter value. If the result is <see langword="false" />,
        /// will be set to <see langword="null" />.</param>
        /// <returns><see langword="true" /> if a value can be supplied; otherwise, <see langword="false" />.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="pi" /> is <see langword="null" />.
        /// </exception>
        public override bool CanSupplyValue(ParameterInfo pi, IComponentContext context, out Func<object> valueProvider)
        {
            if (pi == null) throw new ArgumentNullException(nameof(pi));

            bool hasDefaultValue;
            var tryToGetDefaultValue = true;
            try
            {
                // Workaround for https://github.com/dotnet/corefx/issues/17943
                if (pi.Member.DeclaringType?.GetTypeInfo().Assembly.IsDynamic ?? true)
                {
                    hasDefaultValue = pi.DefaultValue != null && pi.HasDefaultValue;
                }
                else
                {
                    hasDefaultValue = pi.HasDefaultValue;
                }
            }
            catch (FormatException) when (pi.ParameterType == typeof(DateTime))
            {
                // Workaround for https://github.com/dotnet/corefx/issues/12338
                // If HasDefaultValue throws FormatException for DateTime
                // we expect it to have default value
                hasDefaultValue = true;
                tryToGetDefaultValue = false;
            }

            if (hasDefaultValue)
            {
                valueProvider = () =>
                {
                    if (!tryToGetDefaultValue)
                    {
                        return default(DateTime);
                    }

                    var defaultValue = pi.DefaultValue;

                    // Workaround for https://github.com/dotnet/corefx/issues/11797
                    if (defaultValue == null && pi.ParameterType.GetTypeInfo().IsValueType)
                    {
                        defaultValue = Activator.CreateInstance(pi.ParameterType);
                    }

                    return defaultValue;
                };

                return true;
            }

            valueProvider = null;
            return false;
        }
    }
}
