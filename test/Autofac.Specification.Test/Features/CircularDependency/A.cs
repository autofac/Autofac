// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Specification.Test.Features.CircularDependency;

public class A : IA
{
    public A(IC c)
    {
    }
}
