// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Util.Cache;

/// <summary>
/// Defines the possible reflection cache usage types.
/// </summary>
[Flags]
public enum ReflectionCacheUsage
{
    /// <summary>
    /// The cache isn't used (not generally a valid value).
    /// </summary>
    None = 0x00,

    /// <summary>
    /// The cache is used at registration time.
    /// </summary>
    Registration = 0x01,

    /// <summary>
    /// The cache is used after registration, during registration.
    /// </summary>
    Resolution = 0x02,

    /// <summary>
    /// The cache is used during all stages (registration and resolution).
    /// </summary>
    All = Registration | Resolution,
}
