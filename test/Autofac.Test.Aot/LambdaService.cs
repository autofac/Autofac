// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

/// <summary>
/// Verifies lambda registration - the recommended AOT-friendly registration pattern.
/// </summary>
internal sealed class LambdaService
{
    public LambdaService(IDependency dependency)
    {
        Dependency = dependency;
    }

    public IDependency Dependency
    {
        get;
    }
}
