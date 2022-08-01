// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Core.Resolving.Middleware;

/// <summary>
/// Allows configuration of default middleware.
/// </summary>
public static class DefaultMiddlewareConfiguration
{
    /// <summary>
    /// Universally disables the circular dependency check on every resolution. Instead,
    /// a circular dependency may be caught after many loops and may cause undesirable
    /// side effects, however resolution performance should improve with deep resolution chains.
    /// </summary>
    public static void UnsafeDisableProactiveCircularDependencyChecks()
    {
        CircularDependencyDetectorMiddleware.Default.UnsafeDisableProactiveCircularDependencyChecks();
    }

    /// <summary>
    /// Enable the default circular dependency checks.
    /// </summary>
    public static void EnableProactiveCircularDependencyChecks()
    {
        CircularDependencyDetectorMiddleware.Default.EnableProactiveCircularDependencyChecks();
    }
}
