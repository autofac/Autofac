// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Core;

/// <summary>
/// Helper methods for ensuring keyed resolve requests carry the requested key as a parameter.
/// </summary>
internal static class KeyedServiceParameterInjector
{
    /// <summary>
    /// Ensures keyed service requests carry their associated key with the parameter sequence.
    /// </summary>
    /// <param name="service">The service being resolved.</param>
    /// <param name="parameters">The parameters supplied by the caller.</param>
    /// <returns>An enumerable that exposes the keyed service key when appropriate.</returns>
    public static IEnumerable<Parameter> AddKeyedServiceParameter(Service service, IEnumerable<Parameter> parameters)
    {
        if (service == null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        if (service is not KeyedService keyedService)
        {
            return parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        return AddKeyedServiceParameter(keyedService.ServiceKey, parameters);
    }

    /// <summary>
    /// Ensures keyed service requests carry their associated key with the parameter sequence.
    /// </summary>
    /// <param name="service">The service being resolved.</param>
    /// <param name="parameters">The parameters supplied by the caller.</param>
    /// <param name="registration">The component registration (if known).</param>
    /// <returns>An enumerable that exposes the keyed service key when appropriate.</returns>
    public static IEnumerable<Parameter> AddKeyedServiceParameter(Service service, IEnumerable<Parameter> parameters, IComponentRegistration? registration)
    {
        if (service == null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        if (parameters == null)
        {
            throw new ArgumentNullException(nameof(parameters));
        }

        if (service is not KeyedService keyedService || KeyedService.IsAnyKey(keyedService.ServiceKey))
        {
            // It's not a keyed service OR it's registered with AnyKey.
            return parameters;
        }

        if (registration?.Activator is Autofac.Core.Activators.Reflection.ReflectionActivator reflectionActivator &&
            !reflectionActivator.RequiresServiceKeyParameter)
        {
            return parameters;
        }

        return AddKeyedServiceParameter(keyedService.ServiceKey, parameters);
    }

    /// <summary>
    /// Ensures keyed service requests carry their associated key with the parameter sequence.
    /// </summary>
    /// <param name="serviceKey">The keyed service key.</param>
    /// <param name="parameters">The parameters supplied by the caller.</param>
    /// <returns>An enumerable that exposes the keyed service key when appropriate.</returns>
    public static IEnumerable<Parameter> AddKeyedServiceParameter(object serviceKey, IEnumerable<Parameter> parameters)
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

        if (HasKeyParameter(parameters, serviceKey))
        {
            return parameters;
        }

        return AppendKeyParameter(parameters, serviceKey);
    }

    private static bool HasKeyParameter(IEnumerable<Parameter> parameters, object serviceKey)
    {
        foreach (var parameter in parameters)
        {
            if (parameter is KeyedServiceKeyParameter keyParameter && Equals(keyParameter.ServiceKey, serviceKey))
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<Parameter> AppendKeyParameter(IEnumerable<Parameter> parameters, object serviceKey)
    {
        var keyParameter = new KeyedServiceKeyParameter(serviceKey);

        if (ReferenceEquals(parameters, ResolveRequest.NoParameters))
        {
            return new Parameter[] { keyParameter };
        }

        return parameters.Append(keyParameter);
    }
}
