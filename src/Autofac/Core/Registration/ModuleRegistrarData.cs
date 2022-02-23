// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;

namespace Autofac.Core.Registration;

/// <summary>
/// Data used by <see cref="IModuleRegistrar"/> to support customisations to the module registration process.
/// </summary>
public class ModuleRegistrarData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModuleRegistrarData"/> class.
    /// </summary>
    /// <param name="callback">The callback for the registrar.</param>
    public ModuleRegistrarData(DeferredCallback callback)
    {
        Callback = callback;
    }

    /// <summary>
    /// Gets the callback invoked when the collection of modules attached to this registrar are registered.
    /// </summary>
    public DeferredCallback Callback { get; }
}
