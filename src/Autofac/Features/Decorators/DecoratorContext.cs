// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;

namespace Autofac.Features.Decorators;

/// <summary>
/// Implements the decorator context, exposing the state of the decoration process.
/// </summary>
public sealed class DecoratorContext : IDecoratorContext
{
    private readonly IComponentContext _componentContext;

    // Built from AppliedDecorators on first read, since most decorations never read it.
    private Type[]? _appliedDecoratorTypes;

    private DecoratorContext(
        IComponentContext componentContext,
        Type implementationType,
        Type serviceType,
        object currentInstance,
        object[]? appliedDecorators = null)
    {
        _componentContext = componentContext;
        ImplementationType = implementationType;
        ServiceType = serviceType;
        CurrentInstance = currentInstance;
        AppliedDecorators = appliedDecorators ?? Array.Empty<object>();
        _appliedDecoratorTypes = appliedDecorators is null ? Array.Empty<Type>() : null;
    }

    /// <inheritdoc />
    public Type ImplementationType
    {
        get; private set;
    }

    /// <inheritdoc />
    public Type ServiceType
    {
        get; private set;
    }

    /// <inheritdoc />
    public IReadOnlyList<Type> AppliedDecoratorTypes => _appliedDecoratorTypes ??= GetAppliedDecoratorTypes();

    /// <inheritdoc />
    public IReadOnlyList<object> AppliedDecorators
    {
        get; private set;
    }

    /// <inheritdoc />
    public object CurrentInstance
    {
        get; private set;
    }

    /// <inheritdoc />
    public IComponentRegistry ComponentRegistry => _componentContext.ComponentRegistry;

    /// <inheritdoc />
    public object ResolveComponent(in ResolveRequest request) => _componentContext.ResolveComponent(request);

    /// <summary>
    /// Create a new <see cref="DecoratorContext"/>.
    /// </summary>
    /// <param name="componentContext">The component context this decorator context sits in.</param>
    /// <param name="implementationType">The type of the concrete implementation.</param>
    /// <param name="serviceType">The service type being decorated.</param>
    /// <param name="implementationInstance">The instance of the implementation to be decorated.</param>
    /// <returns>A new decorator context.</returns>
    internal static DecoratorContext Create(IComponentContext componentContext, Type implementationType, Type serviceType, object implementationInstance)
    {
        return new DecoratorContext(componentContext, implementationType, serviceType, implementationInstance);
    }

    /// <summary>
    /// Creates a new decorator context that consumes the specified decorator instance.
    /// </summary>
    /// <param name="decoratorInstance">The decorated instance.</param>
    /// <returns>A new context.</returns>
    internal DecoratorContext UpdateContext(object decoratorInstance)
    {
        var count = AppliedDecorators.Count;
        var appliedDecorators = new object[count + 1];

        for (var i = 0; i < count; i++)
        {
            appliedDecorators[i] = AppliedDecorators[i];
        }

        appliedDecorators[count] = decoratorInstance;

        return new DecoratorContext(_componentContext, ImplementationType, ServiceType, decoratorInstance, appliedDecorators);
    }

    private Type[] GetAppliedDecoratorTypes()
    {
        var appliedDecoratorTypes = new Type[AppliedDecorators.Count];

        for (var i = 0; i < appliedDecoratorTypes.Length; i++)
        {
            appliedDecoratorTypes[i] = AppliedDecorators[i].GetType();
        }

        return appliedDecoratorTypes;
    }
}
