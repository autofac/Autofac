// This software is part of the Autofac IoC container
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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

namespace Autofac.Core.Activators.Reflection
{
    /// <summary>
    /// Represents the outcome of a single bind attempt by a <see cref="ConstructorBinder"/>.
    /// </summary>
    [SuppressMessage(
        "Performance",
        "CA1815:Override equals and operator equals on value types",
        Justification = "Comparison of two BoundConstructor instances has no meaning.")]
    public class BoundConstructor
    {
        private readonly Func<object?[], object>? _factory;
        private readonly Func<object?>[]? _valueRetrievers;
        private readonly ParameterInfo? _firstNonBindableParameter;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundConstructor"/> class for a successful bind.
        /// </summary>
        /// <param name="constructor">The constructor.</param>
        /// <param name="factory">The instance factory.</param>
        /// <param name="valueRetrievers">The set of value-retrieval functions.</param>
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Validated in constructor.")]
        public static BoundConstructor ForBindSuccess(ConstructorInfo constructor, Func<object?[], object> factory, Func<object?>[] valueRetrievers)
            => new BoundConstructor(constructor, factory, valueRetrievers);

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundConstructor"/> class, for an unsuccessful bind.
        /// </summary>
        /// <param name="constructor">The constructor.</param>
        /// <param name="totalArgumentCount">The total count of arguments to the constructor.</param>
        /// <param name="firstNonBindableParameter">The first parameter that prevented binding.</param>
        public static BoundConstructor ForBindFailure(ConstructorInfo constructor, int totalArgumentCount, ParameterInfo firstNonBindableParameter) =>
            new BoundConstructor(constructor, totalArgumentCount, firstNonBindableParameter);

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundConstructor"/> class for a successful bind.
        /// </summary>
        /// <param name="constructor">The constructor.</param>
        /// <param name="factory">The instance factory.</param>
        /// <param name="valueRetrievers">The set of value-retrieval functions.</param>
        internal BoundConstructor(ConstructorInfo constructor, Func<object?[], object> factory, Func<object?>[] valueRetrievers)
        {
            CanInstantiate = true;
            TargetConstructor = constructor ?? throw new ArgumentNullException(nameof(constructor));
            ArgumentCount = valueRetrievers.Length;
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _valueRetrievers = valueRetrievers ?? throw new ArgumentNullException(nameof(valueRetrievers));
            _firstNonBindableParameter = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundConstructor"/> class, for an unsuccessful bind.
        /// </summary>
        /// <param name="constructor">The constructor.</param>
        /// <param name="totalArgumentCount">The total count of arguments to the constructor.</param>
        /// <param name="firstNonBindableParameter">The first parameter that prevented binding.</param>
        internal BoundConstructor(ConstructorInfo constructor, int totalArgumentCount, ParameterInfo firstNonBindableParameter)
        {
            CanInstantiate = false;
            TargetConstructor = constructor ?? throw new ArgumentNullException(nameof(constructor));
            ArgumentCount = totalArgumentCount;
            _firstNonBindableParameter = firstNonBindableParameter ?? throw new ArgumentNullException(nameof(firstNonBindableParameter));
            _factory = null;
            _valueRetrievers = null;
        }

        /// <summary>
        /// Gets the constructor on the target type. The actual constructor used
        /// might differ, e.g. if using a dynamic proxy.
        /// </summary>
        public ConstructorInfo TargetConstructor { get; }

        /// <summary>
        /// Gets the total number of arguments for the bound constructor.
        /// </summary>
        public int ArgumentCount { get; }

        /// <summary>
        /// Gets a value indicating whether the binding is valid.
        /// </summary>
        public bool CanInstantiate { get; }

        /// <summary>
        /// Invoke the constructor with the parameter bindings.
        /// </summary>
        /// <returns>The constructed instance.</returns>
        public object Instantiate()
        {
            if (!CanInstantiate)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, BoundConstructorResources.CannotInstantitate, this.Description));
            }

            var values = new object?[_valueRetrievers!.Length];
            for (var i = 0; i < _valueRetrievers.Length; ++i)
            {
                values[i] = _valueRetrievers[i]();
            }

            try
            {
                return _factory!(values);
            }
            catch (TargetInvocationException ex)
            {
                throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, BoundConstructorResources.ExceptionDuringInstantiation, TargetConstructor, TargetConstructor.DeclaringType.Name), ex.InnerException);
            }
            catch (Exception ex)
            {
                throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, BoundConstructorResources.ExceptionDuringInstantiation, TargetConstructor, TargetConstructor.DeclaringType.Name), ex);
            }
        }

        /// <summary>
        /// Gets a description of the constructor parameter binding.
        /// </summary>
        public string Description => CanInstantiate
            ? string.Format(CultureInfo.CurrentCulture, BoundConstructorResources.BoundConstructor, TargetConstructor)
            : string.Format(CultureInfo.CurrentCulture, BoundConstructorResources.NonBindableConstructor, TargetConstructor, _firstNonBindableParameter);

        /// <summary>Returns a System.String that represents the current System.Object.</summary>
        /// <returns>A System.String that represents the current System.Object.</returns>
        public override string ToString()
        {
            return Description;
        }
    }
}
