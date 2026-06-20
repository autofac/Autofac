// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;

namespace Autofac.Core.Activators.Reflection;

/// <summary>
/// Finds constructors that match a finder function.
/// </summary>
public class DefaultConstructorFinder : IConstructorFinder
{
    private readonly Func<Type, ConstructorInfo[]> _finder;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultConstructorFinder" /> class.
    /// </summary>
    /// <remarks>
    /// Default to selecting all public constructors.
    /// </remarks>
    public DefaultConstructorFinder()
      : this(GetDefaultPublicConstructors)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultConstructorFinder" /> class.
    /// </summary>
    /// <param name="finder">The finder function.</param>
    public DefaultConstructorFinder(Func<Type, ConstructorInfo[]> finder)
    {
        _finder = finder ?? throw new ArgumentNullException(nameof(finder));
    }

    /// <summary>
    /// Finds suitable constructors on the target type.
    /// </summary>
    /// <param name="targetType">Type to search for constructors.</param>
    /// <returns>Suitable constructors.</returns>
    public ConstructorInfo[] FindConstructors(Type targetType)
    {
        return _finder(targetType);
    }

    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2067:UnrecognizedReflectionPattern",
        Justification = "This is the default constructor finder used by reflection activation. The target type's public constructors are preserved by the activation contract (ActivatorMemberTypes) flowed through the registration APIs. The finder is invoked via a Func<Type, ConstructorInfo[]> delegate which cannot carry the annotation, so the requirement is asserted here.")]
    [SuppressMessage("Major Code Smell", "S6612:The lambda parameter should be used instead of capturing arguments", Justification = "The factory deliberately reads the [DynamicallyAccessedMembers]-annotated 'type' local rather than the unannotated lambda parameter so the trimming contract flows into GetDeclaredPublicConstructors.")]
    private static ConstructorInfo[] GetDefaultPublicConstructors(Type type)
    {
        return ReflectionCacheSet.Shared.Internal.DefaultPublicConstructors
            .GetOrAdd(type, _ => type.GetDeclaredPublicConstructors());
    }
}
