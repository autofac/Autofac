// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Autofac.Core
{
    /// <summary>
    /// Base class for parameters that provide a constant value.
    /// </summary>
    public abstract class ConstantParameter : Parameter
    {
        private readonly Predicate<ParameterInfo> _predicate;

        /// <summary>
        /// Gets the value of the parameter.
        /// </summary>
        public object? Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantParameter"/> class.
        /// </summary>
        /// <param name="value">
        /// The constant parameter value.
        /// </param>
        /// <param name="predicate">
        /// A predicate used to locate the parameter that should be populated by the constant.
        /// </param>
        protected ConstantParameter(object? value, Predicate<ParameterInfo> predicate)
        {
            Value = value;
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        /// <summary>
        /// Returns true if the parameter is able to provide a value to a particular site.
        /// </summary>
        /// <param name="pi">Constructor, method, or property-mutator parameter.</param>
        /// <param name="context">The component context in which the value is being provided.</param>
        /// <param name="valueProvider">If the result is true, the valueProvider parameter will
        /// be set to a function that will lazily retrieve the parameter value. If the result is false,
        /// will be set to null.</param>
        /// <returns>True if a value can be supplied; otherwise, false.</returns>
        public override bool CanSupplyValue(ParameterInfo pi, IComponentContext context, [NotNullWhen(returnValue: true)] out Func<object?>? valueProvider)
        {
            if (pi == null)
            {
                throw new ArgumentNullException(nameof(pi));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (_predicate(pi))
            {
                valueProvider = () => Value;
                return true;
            }

            valueProvider = null;
            return false;
        }
    }
}
