// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac;

/// <summary>
/// The details of an individual request to resolve a service.
/// </summary>
public readonly struct ResolveRequest : IEquatable<ResolveRequest>
{
    /// <summary>
    /// Shared constant value defining an empty set of parameters.
    /// </summary>
    internal static readonly IEnumerable<Parameter> NoParameters = Enumerable.Empty<Parameter>();

    /// <summary>
    /// Initializes a new instance of the <see cref="ResolveRequest"/> struct.
    /// </summary>
    /// <param name="service">The service being resolved.</param>
    /// <param name="serviceRegistration">The component registration for the service.</param>
    /// <param name="parameters">The parameters used when resolving the service.</param>
    /// <param name="decoratorTarget">The target component to be decorated.</param>
    public ResolveRequest(Service service, ServiceRegistration serviceRegistration, IEnumerable<Parameter> parameters, IComponentRegistration? decoratorTarget = null)
    {
        Service = service;
        Registration = serviceRegistration.Registration;
        ResolvePipeline = serviceRegistration.Pipeline;
        Parameters = parameters;
        DecoratorTarget = decoratorTarget;
    }

    /// <summary>
    /// Gets the service being resolved.
    /// </summary>
    public Service Service { get; }

    /// <summary>
    /// Gets the component registration for the service being resolved. This may be null if a service is being supplied without registrations.
    /// </summary>
    public IComponentRegistration Registration { get; }

    /// <summary>
    /// Gets the resolve pipeline for the request.
    /// </summary>
    public IResolvePipeline ResolvePipeline { get; }

    /// <summary>
    /// Gets the parameters used when resolving the service.
    /// </summary>
    public IEnumerable<Parameter> Parameters { get; }

    /// <summary>
    /// Gets the component registration for the decorator target if configured.
    /// </summary>
    public IComponentRegistration? DecoratorTarget { get; }

    /// <inheritdoc/>
    public override bool Equals(object? obj) =>
        obj is ResolveRequest other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(ResolveRequest other) =>
        Service == other.Service && Registration == other.Registration && ResolvePipeline == other.ResolvePipeline && Parameters == other.Parameters && DecoratorTarget == other.DecoratorTarget;

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator ==(ResolveRequest left, ResolveRequest right) => left.Equals(right);

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator !=(ResolveRequest left, ResolveRequest right) =>
        !(left == right);

    /// <inheritdoc/>
    public override int GetHashCode() =>
        Service.GetHashCode() ^ Registration.GetHashCode() ^ ResolvePipeline.GetHashCode() ^ Parameters.GetHashCode() ^ (DecoratorTarget?.GetHashCode() ?? 0);
}
