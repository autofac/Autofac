// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
        /// <param name="binder">The binder that generated this binding.</param>
        /// <param name="factory">The instance factory.</param>
        /// <param name="valueRetrievers">The set of value-retrieval functions.</param>
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Validated in constructor.")]
        public static BoundConstructor ForBindSuccess(ConstructorBinder binder, Func<object?[], object> factory, Func<object?>[] valueRetrievers)
            => new BoundConstructor(binder, factory, valueRetrievers);

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundConstructor"/> class, for an unsuccessful bind.
        /// </summary>
        /// <param name="binder">The binder that generated this binding.</param>
        /// <param name="firstNonBindableParameter">The first parameter that prevented binding.</param>
        public static BoundConstructor ForBindFailure(ConstructorBinder binder, ParameterInfo firstNonBindableParameter) =>
            new BoundConstructor(binder, firstNonBindableParameter);

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundConstructor"/> class for a successful bind.
        /// </summary>
        /// <param name="binder">The binder that generated this binding.</param>
        /// <param name="factory">The instance factory.</param>
        /// <param name="valueRetrievers">The set of value-retrieval functions.</param>
        internal BoundConstructor(ConstructorBinder binder, Func<object?[], object> factory, Func<object?>[] valueRetrievers)
        {
            CanInstantiate = true;
            Binder = binder;
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _valueRetrievers = valueRetrievers ?? throw new ArgumentNullException(nameof(valueRetrievers));
            _firstNonBindableParameter = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundConstructor"/> class, for an unsuccessful bind.
        /// </summary>
        /// <param name="binder">The binder that generated this binding.</param>
        /// <param name="firstNonBindableParameter">The first parameter that prevented binding.</param>
        internal BoundConstructor(ConstructorBinder binder, ParameterInfo firstNonBindableParameter)
        {
            Binder = binder;
            CanInstantiate = false;
            _firstNonBindableParameter = firstNonBindableParameter ?? throw new ArgumentNullException(nameof(firstNonBindableParameter));
            _factory = null;
            _valueRetrievers = null;
        }

        /// <summary>
        /// Gets the binder that created this binding.
        /// </summary>
        public ConstructorBinder Binder { get; }

        /// <summary>
        /// Gets the constructor on the target type. The actual constructor used
        /// might differ, e.g. if using a dynamic proxy.
        /// </summary>
        public ConstructorInfo TargetConstructor => Binder.Constructor;

        /// <summary>
        /// Gets the total number of arguments for the bound constructor.
        /// </summary>
        public int ArgumentCount => Binder.ParameterCount;

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
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, BoundConstructorResources.CannotInstantitate, Description));
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
                throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, BoundConstructorResources.ExceptionDuringInstantiation, TargetConstructor, TargetConstructor.DeclaringType!.Name), ex.InnerException);
            }
            catch (Exception ex)
            {
                throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, BoundConstructorResources.ExceptionDuringInstantiation, TargetConstructor, TargetConstructor.DeclaringType!.Name), ex);
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
