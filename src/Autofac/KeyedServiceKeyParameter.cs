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
    private readonly bool _allowInjection;

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyedServiceKeyParameter"/> class.
    /// </summary>
    /// <param name="serviceKey">The keyed service key associated with the resolve operation.</param>
    /// <param name="allowInjection">Indicates whether this parameter may satisfy constructor arguments.</param>
    public KeyedServiceKeyParameter(object serviceKey, bool allowInjection = false)
    {
        ServiceKey = serviceKey ?? throw new ArgumentNullException(nameof(serviceKey));
        _allowInjection = allowInjection;
    }

    /// <summary>
    /// Gets the keyed service key value.
    /// </summary>
    public object ServiceKey { get; }

    /// <summary>
    /// Creates a copy of this parameter that can satisfy constructor arguments.
    /// </summary>
    /// <returns>A parameter instance capable of providing the keyed service value to constructors.</returns>
    public KeyedServiceKeyParameter ForConstructorInjection()
        => _allowInjection ? this : new KeyedServiceKeyParameter(ServiceKey, allowInjection: true);

    /// <inheritdoc />
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

        if (_allowInjection && pi.ParameterType.IsInstanceOfType(ServiceKey))
        {
            valueProvider = () => ServiceKey;
            return true;
        }

        valueProvider = null;
        return false;
    }
}
