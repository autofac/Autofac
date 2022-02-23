// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Test.Scenarios.Parameterisation;

public class Parameterised
{
    public string A { get; private set; }

    public int B { get; private set; }

    public Parameterised(string a, int b)
    {
        A = a;
        B = b;
    }
}
