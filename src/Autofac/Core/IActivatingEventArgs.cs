// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Core;

/// <summary>
/// Fired after the construction of an instance but before that instance
/// is shared with any other or any members are invoked on it.
/// </summary>
/// <typeparam name="T">The type of instance being used to satisfy the request.</typeparam>
[SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Event args naming here occurred before the rule was created.")]
public interface IActivatingEventArgs<out T>
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
    /// Gets the instance that will be used to satisfy the request.
    /// </summary>
    T Instance { get; }

    /// <summary>
    /// Gets the parameters supplied to the activator.
    /// </summary>
    IEnumerable<Parameter> Parameters { get; }

    /// <summary>
    /// The instance can be replaced if needed, e.g. by an interface proxy.
    /// </summary>
    /// <param name="instance">The object to use instead of the activated instance.</param>
    void ReplaceInstance(object instance);
}
