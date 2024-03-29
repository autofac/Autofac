﻿// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Core;

/// <summary>
/// Base exception type thrown whenever the dependency resolution process fails. This is a fatal
/// exception, as Autofac is unable to 'roll back' changes to components that may have already
/// been made during the operation. For example, 'on activated' handlers may have already been
/// fired, or 'single instance' components partially constructed.
/// </summary>
public class DependencyResolutionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DependencyResolutionException" /> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public DependencyResolutionException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DependencyResolutionException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public DependencyResolutionException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
