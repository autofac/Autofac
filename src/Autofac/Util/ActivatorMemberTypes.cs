// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Util;

/// <summary>
/// Centralizes the <see cref="DynamicallyAccessedMemberTypes"/> contract for implementation
/// types that Autofac activates through reflection, so that every annotation across the
/// codebase advertises an identical, trim/AOT-consistent member set.
/// </summary>
/// <remarks>
/// <para>
/// The contract is <see cref="DynamicallyAccessedMemberTypes.PublicConstructors"/> plus
/// <see cref="DynamicallyAccessedMemberTypes.PublicProperties"/>:
/// </para>
/// <list type="bullet">
/// <item>
/// Public constructors back the default activation path (the
/// <see cref="Core.Activators.Reflection.DefaultConstructorFinder"/>), so they must be
/// preserved.
/// </item>
/// <item>
/// Public properties back the default property-injection path
/// (<see cref="Core.DefaultPropertySelector"/> only injects public setters), a
/// first-class Autofac feature that must keep working under trimming and native AOT.
/// </item>
/// </list>
/// <para>
/// Non-public constructors (reachable only via a custom
/// <see cref="Core.Activators.Reflection.IConstructorFinder"/>) and non-public properties
/// (reachable only via a custom <see cref="Core.IPropertySelector"/>) are
/// intentionally excluded: those paths are documented as not trim/AOT-safe, and the
/// custom-selector registration APIs already carry <c>[RequiresUnreferencedCode]</c>.
/// </para>
/// </remarks>
internal static class ActivatorMemberTypes
{
    /// <summary>
    /// The member set preserved for reflection-activated implementation types:
    /// <see cref="DynamicallyAccessedMemberTypes.PublicConstructors"/> |
    /// <see cref="DynamicallyAccessedMemberTypes.PublicProperties"/>.
    /// </summary>
    public const DynamicallyAccessedMemberTypes ActivatedType =
        DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties;
}
