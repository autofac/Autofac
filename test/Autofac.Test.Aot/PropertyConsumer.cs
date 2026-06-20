// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

/// <summary>
/// Verifies public-property injection (preserved by the AOT contract).
/// </summary>
internal sealed class PropertyConsumer
{
    public IDependency? Dependency
    {
        get; set;
    }
}
