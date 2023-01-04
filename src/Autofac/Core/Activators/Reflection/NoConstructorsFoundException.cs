// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Globalization;

namespace Autofac.Core.Activators.Reflection;

/// <summary>
/// Exception thrown when no suitable constructors could be found on a type.
/// </summary>
public class NoConstructorsFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NoConstructorsFoundException"/> class.
    /// </summary>
    /// <param name="offendingType">The <see cref="Type"/> whose constructor was not found.</param>
    /// <param name="constructorFinder">The <see cref="IConstructorFinder"/> that was used to scan for the constructors.</param>
    public NoConstructorsFoundException(Type offendingType, IConstructorFinder constructorFinder)
        : this(offendingType, constructorFinder, FormatMessage(offendingType, constructorFinder))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NoConstructorsFoundException"/> class.
    /// </summary>
    /// <param name="offendingType">The <see cref="Type"/> whose constructor was not found.</param>
    /// <param name="constructorFinder">The <see cref="IConstructorFinder"/> that was used to scan for the constructors.</param>
    /// <param name="message">Exception message.</param>
    public NoConstructorsFoundException(Type offendingType, IConstructorFinder constructorFinder, string message)
        : base(message)
    {
        OffendingType = offendingType ?? throw new ArgumentNullException(nameof(offendingType));
        ConstructorFinder = constructorFinder ?? throw new ArgumentNullException(nameof(constructorFinder));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NoConstructorsFoundException"/> class.
    /// </summary>
    /// <param name="offendingType">The <see cref="Type"/> whose constructor was not found.</param>
    /// <param name="constructorFinder">The <see cref="IConstructorFinder"/> that was used to scan for the constructors.</param>
    /// <param name="innerException">The inner exception.</param>
    public NoConstructorsFoundException(Type offendingType, IConstructorFinder constructorFinder, Exception innerException)
        : this(offendingType, constructorFinder, FormatMessage(offendingType, constructorFinder), innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NoConstructorsFoundException"/> class.
    /// </summary>
    /// <param name="offendingType">The <see cref="Type"/> whose constructor was not found.</param>
    /// <param name="constructorFinder">The <see cref="IConstructorFinder"/> that was used to scan for the constructors.</param>
    /// <param name="message">Exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public NoConstructorsFoundException(Type offendingType, IConstructorFinder constructorFinder, string message, Exception innerException)
        : base(message, innerException)
    {
        OffendingType = offendingType ?? throw new ArgumentNullException(nameof(offendingType));
        ConstructorFinder = constructorFinder ?? throw new ArgumentNullException(nameof(constructorFinder));
    }

    /// <summary>
    /// Gets the finder used when locating constructors.
    /// </summary>
    /// <value>
    /// An <see cref="IConstructorFinder"/> that was used when scanning the
    /// <see cref="OffendingType"/> to find constructors.
    /// </value>
    public IConstructorFinder ConstructorFinder { get; private set; }

    /// <summary>
    /// Gets the type without found constructors.
    /// </summary>
    /// <value>
    /// A <see cref="Type"/> that was processed by the
    /// <see cref="ConstructorFinder"/> and was determined to have no available
    /// constructors.
    /// </value>
    public Type OffendingType { get; private set; }

    private static string FormatMessage(Type offendingType, IConstructorFinder constructorFinder)
    {
        if (offendingType == null)
        {
            throw new ArgumentNullException(nameof(offendingType));
        }

        if (constructorFinder == null)
        {
            throw new ArgumentNullException(nameof(constructorFinder));
        }

        return string.Format(CultureInfo.CurrentCulture, NoConstructorsFoundExceptionResources.Message, offendingType.FullName, constructorFinder.GetType().FullName);
    }
}
