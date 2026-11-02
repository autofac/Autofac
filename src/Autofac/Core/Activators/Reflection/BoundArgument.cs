// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Core.Activators.Reflection;

/// <summary>
/// Supplies the value of one constructor argument for a <see cref="BoundConstructor"/>: either a value retriever from
/// a <see cref="Parameter"/>, or, for an autowired argument, the service and registration to resolve, which avoids
/// allocating a delegate and closure per argument.
/// </summary>
internal readonly struct BoundArgument
{
    private readonly Func<object?>? _valueRetriever;
    private readonly Service? _service;
    private readonly ServiceRegistration _implementation;

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundArgument"/> struct that invokes a value retriever.
    /// </summary>
    /// <param name="valueRetriever">The value retriever supplied by a <see cref="Parameter"/>.</param>
    public BoundArgument(Func<object?> valueRetriever)
    {
        _valueRetriever = valueRetriever;
        _service = null;
        _implementation = default;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundArgument"/> struct that resolves an autowired service.
    /// </summary>
    /// <param name="service">The service that supplies the value.</param>
    /// <param name="implementation">The registration for <paramref name="service"/>.</param>
    public BoundArgument(Service service, ServiceRegistration implementation)
    {
        _valueRetriever = null;
        _service = service;
        _implementation = implementation;
    }

    /// <summary>
    /// Gets the argument value.
    /// </summary>
    /// <param name="context">The context to resolve autowired services from.</param>
    /// <returns>The argument value.</returns>
    public object? GetValue(IComponentContext? context)
    {
        // Discriminate on the service, so a Parameter that supplied a null retriever fails as it always has, on invocation.
        return _service is null
            ? _valueRetriever!()
            : AutowiringParameter.ResolveValue(context!, _service, _implementation);
    }
}
