// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Reflection;

namespace Autofac.Core;

/// <summary>
/// Helper methods for ensuring keyed resolve requests carry the requested key as a parameter.
/// </summary>
internal static partial class KeyedServiceParameterInjector
{
    /// <summary>
    /// Ensures keyed service requests carry their associated key with the parameter sequence.
    /// </summary>
    /// <param name="service">The service being resolved.</param>
    /// <param name="parameters">The parameters supplied by the caller.</param>
    /// <returns>An enumerable that exposes the keyed service key when appropriate.</returns>
    public static IEnumerable<Parameter> EnsureKeyedServiceParameter(Service service, IEnumerable<Parameter> parameters)
    {
        if (service == null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        if (service is not KeyedService keyedService)
        {
            return parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        return EnsureKeyedServiceParameter(keyedService.ServiceKey, parameters);
    }

    /// <summary>
    /// Ensures keyed service requests carry their associated key with the parameter sequence.
    /// </summary>
    /// <param name="serviceKey">The keyed service key.</param>
    /// <param name="parameters">The parameters supplied by the caller.</param>
    /// <returns>An enumerable that exposes the keyed service key when appropriate.</returns>
    public static IEnumerable<Parameter> EnsureKeyedServiceParameter(object serviceKey, IEnumerable<Parameter> parameters)
    {
        if (serviceKey == null)
        {
            throw new ArgumentNullException(nameof(serviceKey));
        }

        if (parameters == null)
        {
            throw new ArgumentNullException(nameof(parameters));
        }

        if (KeyedService.IsAnyKey(serviceKey))
        {
            return parameters;
        }

        if (parameters is IKeyedServiceKeyAccessor accessor &&
            accessor.TryGetServiceKey(out var existingKey) &&
            Equals(existingKey, serviceKey))
        {
            return parameters;
        }

        return new KeyedServiceParameterCollection(parameters, serviceKey);
    }

    /// <summary>
    /// Creates a parameter that can supply the keyed service key to constructor arguments, if available.
    /// </summary>
    /// <param name="parameters">The parameters supplied by the caller.</param>
    /// <returns>A parameter capable of supplying the keyed service key, or null if not available.</returns>
    public static Parameter? TryCreateConstructorParameter(IEnumerable<Parameter> parameters)
    {
        if (parameters is IKeyedServiceKeyAccessor accessor &&
            accessor.TryGetServiceKey(out var key))
        {
            return new KeyedServiceConstructorParameter(key);
        }

        return null;
    }

    /// <summary>
    /// Wraps a set of parameters with an associated keyed service key.
    /// </summary>
    private sealed class KeyedServiceParameterCollection : IEnumerable<Parameter>, IKeyedServiceKeyAccessor
    {
        private readonly IEnumerable<Parameter> _source;
        private readonly object _key;

        public KeyedServiceParameterCollection(IEnumerable<Parameter> source, object key)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _key = key ?? throw new ArgumentNullException(nameof(key));
        }

        public IEnumerator<Parameter> GetEnumerator() => _source.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool TryGetServiceKey([NotNullWhen(returnValue: true)] out object? key)
        {
            key = _key;
            return true;
        }
    }

    /// <summary>
    /// Supplies the keyed service key value to constructor parameters when matching by type.
    /// </summary>
    private sealed class KeyedServiceConstructorParameter : Parameter
    {
        private readonly object _key;

        public KeyedServiceConstructorParameter(object key)
        {
            _key = key ?? throw new ArgumentNullException(nameof(key));
        }

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

            if (pi.ParameterType.IsAssignableFrom(_key.GetType()))
            {
                valueProvider = () => _key;
                return true;
            }

            valueProvider = null;
            return false;
        }
    }
}
