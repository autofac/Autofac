﻿// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Test.Util;

namespace Autofac.Test.Scenarios.Graph1;

// In the below scenario, B1 depends on A1, CD depends on A1 and B1,
// and E depends on IC1 and B1.
public class B1 : DisposeTracker
{
    public B1(A1 a)
    {
        A = a;
    }

    public A1 A { get; private set; }
}
