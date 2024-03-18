// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Test.Scenarios.Parameterization;

public class Parameterized
{
    public string A { get; private set; }

    public int B { get; private set; }

    public Parameterized(string a, int b)
    {
        A = a;
        B = b;
    }
}
