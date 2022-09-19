// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Core;

/// <summary>
/// Services are the lookup keys used to locate component instances.
/// </summary>
internal class IsolatedService : Service
{
    public IsolatedService(Service service)
    {
        Service = service;
    }

    /// <summary>
    /// Contains the actual service that has been isolated.
    /// </summary>
    public Service Service { get; }

    /// <inheritdoc />
    public override string Description => Service.Description;
}
