// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Core;

/// <summary>
/// Defines an isolated wrapper for a regular service.
/// </summary>
internal class ScopeIsolatedService : Service
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScopeIsolatedService"/> class.
    /// </summary>
    /// <param name="service">The service to wrap for isolation.</param>
    public ScopeIsolatedService(Service service)
    {
        Service = service;
    }

    /// <summary>
    /// Gets the actual service that has been isolated.
    /// </summary>
    public Service Service { get; }

    /// <inheritdoc />
    public override string Description => Service.Description;
}
