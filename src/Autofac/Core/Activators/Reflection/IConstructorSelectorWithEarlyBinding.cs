// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Core.Activators.Reflection;

/// <summary>
/// Defines an interface that indicates a constructor selector can attempt to
/// determine the correct constructor early, as the container is built,
/// without needing to understand the set of parameters in each resolve.
/// </summary>
public interface IConstructorSelectorWithEarlyBinding : IConstructorSelector
{
    /// <summary>
    /// Given the set of all found constructors for a registration, try and
    /// select the correct single <see cref="ConstructorBinder"/> to use.
    /// </summary>
    /// <param name="constructorBinders">
    /// The set of binders for all constructors found by
    /// <see cref="IConstructorFinder"/> on the registration.
    /// </param>
    /// <returns>
    /// The single, correct binder to use. If the method returns null, this
    /// indicates the selector requires resolve-time parameters, and the default
    /// <see cref="IConstructorSelector.SelectConstructorBinding"/> method will
    /// be invoked.
    /// </returns>
    ConstructorBinder? SelectConstructorBinder(ConstructorBinder[] constructorBinders);
}
