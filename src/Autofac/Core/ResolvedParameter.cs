// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Autofac.Core
{
    /// <summary>
    /// Flexible parameter type allows arbitrary values to be retrieved
    /// from the resolution context.
    /// </summary>
    public class ResolvedParameter : Parameter
    {
        private readonly Func<ParameterInfo, IComponentContext, bool> _predicate;
        private readonly Func<ParameterInfo, IComponentContext, object?> _valueAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolvedParameter"/> class.
        /// </summary>
        /// <param name="predicate">A predicate that determines which parameters on a constructor will be supplied by this instance.</param>
        /// <param name="valueAccessor">A function that supplies the parameter value given the context.</param>
        public ResolvedParameter(Func<ParameterInfo, IComponentContext, bool> predicate, Func<ParameterInfo, IComponentContext, object?> valueAccessor)
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
            _valueAccessor = valueAccessor ?? throw new ArgumentNullException(nameof(valueAccessor));
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

            if (_predicate(pi, context))
            {
                valueProvider = () => _valueAccessor(pi, context);
                return true;
            }

            valueProvider = null;
            return false;
        }

        /// <summary>
        /// Construct a <see cref="ResolvedParameter"/> that will match parameters of type
        /// <typeparamref name="TService"/> and resolve for those parameters an implementation
        /// registered with the name <paramref name="serviceName"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the parameter to match.</typeparam>
        /// <param name="serviceName">The name of the matching service to resolve.</param>
        /// <returns>A configured <see cref="ResolvedParameter"/> instance.</returns>
        /// <remarks>
        /// </remarks>
        public static ResolvedParameter ForNamed<TService>(string serviceName)
        {
            if (serviceName == null)
            {
                throw new ArgumentNullException(nameof(serviceName));
            }

            return ForKeyed<TService>(serviceName);
        }

        /// <summary>
        /// Construct a <see cref="ResolvedParameter"/> that will match parameters of type
        /// <typeparamref name="TService"/> and resolve for those parameters an implementation
        /// registered with the key <paramref name="serviceKey"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the parameter to match.</typeparam>
        /// <param name="serviceKey">The key of the matching service to resolve.</param>
        /// <returns>A configured <see cref="ResolvedParameter"/> instance.</returns>
        public static ResolvedParameter ForKeyed<TService>(object serviceKey)
        {
            if (serviceKey == null)
            {
                throw new ArgumentNullException(nameof(serviceKey));
            }

            var ks = new KeyedService(serviceKey, typeof(TService));
            return new ResolvedParameter(
                (pi, c) => pi.ParameterType == typeof(TService) && c.IsRegisteredService(ks),
                (pi, c) => c.ResolveService(ks));
        }
    }
}
