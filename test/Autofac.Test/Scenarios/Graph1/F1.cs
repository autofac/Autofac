﻿// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Test.Scenarios.Graph1;

// In the below scenario, B1 depends on A1, CD depends on A1 and B1,
// and E depends on IC1 and B1.
public class F1
{
    public IList<A1> AList { get; private set; }

    public F1(IList<A1> aList)
    {
        AList = aList;
    }
}
