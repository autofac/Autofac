// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core.Resolving.Pipeline;
using Autofac.Features.Decorators;

namespace Autofac.Builder;

/// <summary>
/// Allows a decorator registration to be configured after it has been added to a
/// <see cref="ContainerBuilder"/>.
/// </summary>
/// <typeparam name="TService">The service type being decorated.</typeparam>
/// <remarks>
/// <para>
/// A decorator is not an ordinary registration. The service it exposes is fixed by the
/// decoration machinery, and its lifetime is governed by the registration being decorated.
/// This builder therefore exposes a deliberately smaller surface than
/// <see cref="IRegistrationBuilder{TLimit, TActivatorData, TRegistrationStyle}"/>: there is no
/// <c>As</c>, <c>Named</c>, <c>Keyed</c>, or lifetime configuration, because none of those are
/// meaningful for a decorator.
/// </para>
/// <para>
/// The underlying component registration is not created until the container is built, so
/// configuration may be applied at any point before <see cref="ContainerBuilder.Build(ContainerBuildOptions)"/>.
/// </para>
/// </remarks>
public interface IDecoratorRegistrationBuilder<TService>
{
    /// <summary>
    /// Gets the metadata associated with the decorator registration.
    /// </summary>
    /// <remarks>
    /// Integrations that need to read as well as write metadata (for example, to append to a
    /// list of values already present) can use this dictionary directly; otherwise prefer
    /// <see cref="WithMetadata(string, object?)"/>.
    /// </remarks>
    IDictionary<string, object?> Metadata
    {
        get;
    }

    /// <summary>
    /// Configure the resolve pipeline of the decorator registration.
    /// </summary>
    /// <param name="configurationAction">
    /// An action that can add middleware to the decorator's registration pipeline.
    /// </param>
    /// <returns>The builder, to allow continued configuration.</returns>
    /// <remarks>
    /// Middleware added here runs when the decorator instance itself is activated, which means
    /// it wraps the decorator rather than the component the decorator decorates.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="configurationAction" /> is <see langword="null" />.
    /// </exception>
    IDecoratorRegistrationBuilder<TService> ConfigurePipeline(Action<IResolvePipelineBuilder> configurationAction);

    /// <summary>
    /// Provide a key/value pair of metadata associated with the decorator registration.
    /// </summary>
    /// <param name="key">Key of the metadata item.</param>
    /// <param name="value">Value of the metadata item.</param>
    /// <returns>The builder, to allow continued configuration.</returns>
    IDecoratorRegistrationBuilder<TService> WithMetadata(string key, object? value);

    /// <summary>
    /// Provide a set of metadata values associated with the decorator registration.
    /// </summary>
    /// <param name="properties">The metadata values to add.</param>
    /// <returns>The builder, to allow continued configuration.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="properties" /> is <see langword="null" />.
    /// </exception>
    IDecoratorRegistrationBuilder<TService> WithMetadata(IEnumerable<KeyValuePair<string, object?>> properties);

    /// <summary>
    /// Provide the condition that determines whether the decorator is applied.
    /// </summary>
    /// <param name="condition">
    /// A function that, when provided with an <see cref="IDecoratorContext"/> instance, determines
    /// if the decorator should be applied.
    /// </param>
    /// <returns>The builder, to allow continued configuration.</returns>
    /// <remarks>
    /// This replaces any condition already supplied, including one passed to the
    /// <c>RegisterDecorator</c> method that created this builder.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="condition" /> is <see langword="null" />.
    /// </exception>
    IDecoratorRegistrationBuilder<TService> WithCondition(Func<IDecoratorContext, bool> condition);
}
