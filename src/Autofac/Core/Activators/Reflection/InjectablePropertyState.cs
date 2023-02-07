// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Core.Activators.Reflection;

/// <summary>
/// Structure used to track whether or not we have set a property during activation.
/// </summary>
internal struct InjectablePropertyState
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InjectablePropertyState"/> struct.
    /// </summary>
    /// <param name="property">The property.</param>
    public InjectablePropertyState(InjectableProperty property)
    {
        Property = property;
        Set = false;
    }

    /// <summary>
    /// Gets the property.
    /// </summary>
    public InjectableProperty Property { get; }

    /// <summary>
    /// Gets or sets a value indicating whether this property has already been set.
    /// </summary>
    public bool Set { get; set; }
}
