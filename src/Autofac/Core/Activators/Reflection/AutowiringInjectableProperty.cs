// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;

namespace Autofac.Core.Activators.Reflection;

/// <summary>
/// Holds what <see cref="AutowiringPropertyInjector"/> needs per resolve for one injectable property of a type,
/// so it is looked up once per type instead of on every resolve.
/// </summary>
internal sealed class AutowiringInjectableProperty
{
    private const int Unknown = 0;
    private const int Selected = 1;
    private const int NotSelected = 2;

    private int _selectedByDefault;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutowiringInjectableProperty"/> class.
    /// </summary>
    /// <param name="property">The property, as found on the instance type.</param>
    /// <param name="instanceTypeParameters">The parameters passed when resolving a value for the property.</param>
    public AutowiringInjectableProperty(PropertyInfo property, Parameter[] instanceTypeParameters)
    {
        Property = property;

        // SetMethod will be non-null for an injectable property.
        SetterParameter = property.SetMethod!.GetParameters()[0];
        Service = new TypedService(property.PropertyType);
        InstanceTypeParameters = instanceTypeParameters;
    }

    /// <summary>
    /// Gets the property.
    /// </summary>
    public PropertyInfo Property
    {
        get;
    }

    /// <summary>
    /// Gets the parameter of the property's setter.
    /// </summary>
    public ParameterInfo SetterParameter
    {
        get;
    }

    /// <summary>
    /// Gets the service resolved for the property.
    /// </summary>
    public TypedService Service
    {
        get;
    }

    /// <summary>
    /// Gets the parameters passed when resolving the property's service, which carry the instance type.
    /// The array is shared, so it must not be modified.
    /// </summary>
    public Parameter[] InstanceTypeParameters
    {
        get;
    }

    /// <summary>
    /// Gets or sets the setter delegate, created when the property is first injected.
    /// </summary>
    public Action<object, object?>? Setter
    {
        get; set;
    }

    /// <summary>
    /// Gets a value indicating whether a <see cref="DefaultPropertySelector"/> considers the property at all,
    /// before checking any value already set. That part depends only on the property, so it is computed once.
    /// </summary>
    public bool IsSelectedByDefault
    {
        get
        {
            if (_selectedByDefault == Unknown)
            {
                _selectedByDefault = DefaultPropertySelector.IsSelectable(Property) ? Selected : NotSelected;
            }

            return _selectedByDefault == Selected;
        }
    }
}
