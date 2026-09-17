// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Core.Resolving.Pipeline;
using Autofac.Features.Decorators;

namespace Autofac.Builder;

/// <summary>
/// Default implementation of <see cref="IDecoratorRegistrationBuilder{TService}"/>.
/// </summary>
/// <typeparam name="TService">The service type being decorated.</typeparam>
/// <remarks>
/// The component registration backing a decorator is not created until the container is built.
/// That is what allows configuration (pipeline middleware, metadata, the decoration condition) to
/// be applied after the <c>RegisterDecorator</c> call returns; <see cref="Register"/> snapshots
/// everything at build time.
/// </remarks>
internal sealed class DecoratorRegistrationBuilder<TService> : IDecoratorRegistrationBuilder<TService>
{
    private readonly Type _serviceType;
    private readonly RegistrationData _registrationData;
    private readonly IResolvePipelineBuilder _pipelineBuilder;
    private readonly Func<DecoratorService, IComponentRegistration> _registrationFactory;
    private Func<IDecoratorContext, bool>? _condition;

    /// <summary>
    /// Initializes a new instance of the <see cref="DecoratorRegistrationBuilder{TService}"/> class.
    /// </summary>
    /// <param name="serviceType">The service type being decorated.</param>
    /// <param name="registrationData">The registration data of the decorator registration.</param>
    /// <param name="pipelineBuilder">The resolve pipeline builder of the decorator registration.</param>
    /// <param name="registrationFactory">
    /// A factory that, given the decorator service, produces the decorator's component registration.
    /// This is invoked once, at container build time.
    /// </param>
    /// <param name="condition">The initial decoration condition, if any.</param>
    internal DecoratorRegistrationBuilder(
        Type serviceType,
        RegistrationData registrationData,
        IResolvePipelineBuilder pipelineBuilder,
        Func<DecoratorService, IComponentRegistration> registrationFactory,
        Func<IDecoratorContext, bool>? condition)
    {
        _serviceType = serviceType;
        _registrationData = registrationData;
        _pipelineBuilder = pipelineBuilder;
        _registrationFactory = registrationFactory;
        _condition = condition;
    }

    /// <inheritdoc/>
    public IDictionary<string, object?> Metadata => _registrationData.Metadata;

    /// <inheritdoc/>
    public IDecoratorRegistrationBuilder<TService> ConfigurePipeline(Action<IResolvePipelineBuilder> configurationAction)
    {
        if (configurationAction is null)
        {
            throw new ArgumentNullException(nameof(configurationAction));
        }

        configurationAction(_pipelineBuilder);

        return this;
    }

    /// <inheritdoc/>
    public IDecoratorRegistrationBuilder<TService> WithMetadata(string key, object? value)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        _registrationData.Metadata.Add(key, value);

        return this;
    }

    /// <inheritdoc/>
    public IDecoratorRegistrationBuilder<TService> WithMetadata(IEnumerable<KeyValuePair<string, object?>> properties)
    {
        if (properties == null)
        {
            throw new ArgumentNullException(nameof(properties));
        }

        foreach (var prop in properties)
        {
            WithMetadata(prop.Key, prop.Value);
        }

        return this;
    }

    /// <inheritdoc/>
    public IDecoratorRegistrationBuilder<TService> WithCondition(Func<IDecoratorContext, bool> condition)
    {
        _condition = condition ?? throw new ArgumentNullException(nameof(condition));

        return this;
    }

    /// <summary>
    /// Creates the decorator registration and adds it, along with the decorator middleware, to the
    /// component registry. Invoked from the container builder's configuration callbacks.
    /// </summary>
    /// <param name="registryBuilder">The component registry builder.</param>
    internal void Register(IComponentRegistryBuilder registryBuilder)
    {
        var decoratorService = new DecoratorService(_serviceType, _condition);
        var decoratorRegistration = _registrationFactory(decoratorService);
        var middleware = new DecoratorMiddleware(decoratorService, decoratorRegistration);

        // Not using the ContainerBuilder extension methods here: they queue additional
        // configuration callbacks, and this code is itself running from inside one.
        registryBuilder.AddServiceMiddlewareSource(
            new ServiceWithTypeMiddlewareSource(_serviceType, middleware, MiddlewareInsertionMode.StartOfPhase));

        // Add the decorator to the registry so the pipeline gets built.
        registryBuilder.Register(decoratorRegistration);
    }
}
