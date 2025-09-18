// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Core;

/// <summary>
/// Fired when the activation process for a new instance is complete.
/// </summary>
/// <typeparam name="T">The type of instance being used to satisfy the request.</typeparam>
[SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Event args naming here occurred before the rule was created.")]
public interface IActivatedEventArgs<out T>
{
    /// <summary>
    /// Gets the service being resolved.
    /// </summary>
    Service Service { get; }

    /// <summary>
    /// Gets the context in which the activation occurred.
    /// </summary>
    IComponentContext Context { get; }

    /// <summary>
    /// Gets the component providing the instance.
    /// </summary>
    IComponentRegistration Component { get; }

    /// <summary>
    /// Gets the parameters provided when resolved.
    /// </summary>
    IEnumerable<Parameter> Parameters { get; }

    /// <summary>
    /// Gets the instance that will be used to satisfy the request.
    /// </summary>
    T Instance { get; }
}
