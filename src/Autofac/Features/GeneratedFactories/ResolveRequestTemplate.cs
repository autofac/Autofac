// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;

namespace Autofac.Features.GeneratedFactories;

/// <summary>
/// A generated factory's <see cref="ResolveRequest"/>, missing only the parameters built from the
/// factory's arguments. Passed to the compiled generator as a single reference type because <see cref="ServiceRegistration"/> is a struct, and value-type
/// parameters prevent Mono full AOT from interpreting the generator (#1519).
/// </summary>
internal sealed class ResolveRequestTemplate
{
    private readonly Service _service;
    private readonly ServiceRegistration _registration;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResolveRequestTemplate"/> class.
    /// </summary>
    /// <param name="service">The service the factory resolves.</param>
    /// <param name="registration">The registration that provides the service.</param>
    public ResolveRequestTemplate(Service service, ServiceRegistration registration)
    {
        _service = service;
        _registration = registration;
    }

    /// <summary>
    /// Creates the request that resolves the factory's product.
    /// </summary>
    /// <param name="parameters">The parameters built from the factory's arguments.</param>
    /// <returns>A request for the target service and registration.</returns>
    public ResolveRequest CreateRequest(IEnumerable<Parameter> parameters)
        => new(_service, _registration, parameters);
}
