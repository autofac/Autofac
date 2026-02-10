// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac;

/// <summary>
/// Marks a constructor parameter or property as a target for service key injection.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class ServiceKeyAttribute : Attribute
{
}
