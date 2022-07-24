// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.Serialization;

namespace Autofac.Core;

/// <summary>
/// Exception type thrown whenever there is a circular dependency detected. This occurs when
/// a dependency is resolved twice within the same direct dependency chain.
/// </summary>
[Serializable]
public class CircularDependencyException : DependencyResolutionException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CircularDependencyException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="dependencyGraph">The specific dependency chain which caused the exception,
    /// which may be needed to present the correct dependency context later.</param>
    public CircularDependencyException(string message, string dependencyGraph)
        : base(message)
    {
        DependencyGraph = dependencyGraph;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CircularDependencyException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public CircularDependencyException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CircularDependencyException"/> class.
    /// </summary>
    /// <param name="info">The serialisation info.</param>
    /// <param name="context">The serialisation streaming context.</param>
    protected CircularDependencyException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    /// <summary>
    /// Gets sdfsdf.
    /// </summary>
    public string? DependencyGraph { get; }
}
