// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Core;

namespace Autofac;

/// <summary>
/// Parameter that exposes the keyed service key for the current resolve operation.
/// </summary>
internal sealed class KeyedServiceKeyParameter : Parameter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KeyedServiceKeyParameter"/> class.
    /// </summary>
    /// <param name="serviceKey">The keyed service key associated with the resolve operation.</param>
    public KeyedServiceKeyParameter(object serviceKey)
    {
        ServiceKey = serviceKey ?? throw new ArgumentNullException(nameof(serviceKey));
    }

    /// <summary>
    /// Gets the keyed service key value.
    /// </summary>
    public object ServiceKey { get; }

    /// <inheritdoc/>
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

        if (!ShouldInject(pi))
        {
            valueProvider = null;
            return false;
        }

        valueProvider = () => ServiceKey;
        return true;
    }

    private static bool ShouldInject(ParameterInfo parameter)
    {
        return ServiceKeyAttributeCache.ParameterHasServiceKey(parameter);
    }
}
