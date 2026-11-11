// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Core;

namespace Autofac.Features.Decorators;

/// <summary>
/// Supplies the instance being decorated to constructor parameters it is compatible with.
/// </summary>
/// <remarks>
/// Behaves like a <see cref="ResolvedParameter"/> built from two lambdas, but holds its state in one object
/// instead of a closure and two delegates allocated on every decoration.
/// </remarks>
internal sealed class CompatibleServiceParameter : ResolvedParameter
{
    private static readonly Func<ParameterInfo, IComponentContext, bool> _unusedPredicate = (pi, ctx) => false;
    private static readonly Func<ParameterInfo, IComponentContext, object?> _unusedValueAccessor = (pi, ctx) => null;

    private readonly Type _serviceType;
    private readonly object _instance;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompatibleServiceParameter"/> class.
    /// </summary>
    /// <param name="serviceType">The service type being decorated.</param>
    /// <param name="instance">The instance being decorated.</param>
    public CompatibleServiceParameter(Type serviceType, object instance)
        : base(_unusedPredicate, _unusedValueAccessor)
    {
        _serviceType = serviceType;
        _instance = instance;
    }

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

        if (_serviceType.IsAssignableFrom(pi.ParameterType) && pi.ParameterType.IsInstanceOfType(_instance))
        {
            valueProvider = GetInstance;
            return true;
        }

        valueProvider = null;
        return false;
    }

    private object? GetInstance() => _instance;
}
