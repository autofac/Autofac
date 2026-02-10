// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Core;

/// <summary>
/// Helper methods for ensuring keyed resolve requests carry the requested key as a parameter.
/// </summary>
internal static partial class KeyedServiceParameterUtilities
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

    // Nested types defined in KeyedServiceParameterUtilities.Types.cs.
}
