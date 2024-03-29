﻿// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;
using Autofac.Core.Registration;

namespace Autofac.Features.Scanning;

/// <summary>
/// Base activation data for types located by scanning assemblies.
/// </summary>
public abstract class BaseScanningActivatorData<TActivatorData, TRegistrationStyle> : ReflectionActivatorData
    where TActivatorData : ReflectionActivatorData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseScanningActivatorData{TActivatorData, TRegistrationStyle}"/> class.
    /// </summary>
    /// <param name="configurationActions">Additional actions to be performed on the concrete type registration.</param>
    protected BaseScanningActivatorData(
        ICollection<Action<Type, IRegistrationBuilder<object, TActivatorData, TRegistrationStyle>>> configurationActions)
        : base(typeof(object))
    {
        ConfigurationActions = configurationActions;
    }

    /// <summary>
    /// Gets the additional actions to be performed on the concrete type registrations.
    /// </summary>
    public ICollection<Action<Type, IRegistrationBuilder<object, TActivatorData, TRegistrationStyle>>> ConfigurationActions { get; }

    /// <summary>
    /// Gets the filters applied to the types from the scanned assembly.
    /// </summary>
    public ICollection<Func<Type, bool>> Filters { get; } = new List<Func<Type, bool>>();

    /// <summary>
    /// Gets the actions to be called once the scanning operation is complete.
    /// </summary>
    public ICollection<Action<IComponentRegistryBuilder>> PostScanningCallbacks { get; } = new List<Action<IComponentRegistryBuilder>>();
}
