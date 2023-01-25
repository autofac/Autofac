// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Core.Activators.Reflection;
using Autofac.Util;

namespace Autofac.Core;

/// <summary>
/// A required property by its property info.
/// </summary>
/// <remarks>Internal because we also use this property to cache setter methods and parameter info.</remarks>
internal class RequiredPropertyParameter : AutowiringParameter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RequiredPropertyParameter"/> class.
    /// </summary>
    /// <param name="property">The property info of the property being set.</param>
    public RequiredPropertyParameter(PropertyInfo property)
    {
        Property = property;

        if (property.SetMethod is null)
        {
            throw new ArgumentException("Property does not have a setter", nameof(property));
        }

        Setter = property.SetMethod;

        Parameter = Setter.GetParameters()[0];
    }

    public PropertyInfo Property { get; }

    public MethodInfo Setter { get; }

    public ParameterInfo Parameter { get; }
}
