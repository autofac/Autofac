// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.Scanning;

namespace Autofac;

/// <summary>
/// Adds registration syntax to the <see cref="ContainerBuilder"/> type.
/// </summary>
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
public static partial class RegistrationExtensions
{
    public static void UseReflectionCache(this ContainerBuilder builder, ReflectionCache reflectionCache)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (reflectionCache is null)
        {
            throw new ArgumentNullException(nameof(reflectionCache));
        }

        builder.ComponentRegistryBuilder.ReflectionCache = reflectionCache;
    }
}
