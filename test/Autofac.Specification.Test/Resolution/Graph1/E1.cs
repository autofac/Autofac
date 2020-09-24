// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using Autofac.Specification.Test.Util;

namespace Autofac.Specification.Test.Resolution.Graph1
{
    // In the below scenario, B1 depends on A1, CD depends on A1 and B1,
    // and E depends on IC1 and B1.
    public class E1 : DisposeTracker
    {
        public E1(B1 b, IC1 c)
        {
            B = b;
            C = c;
        }

        public B1 B { get; private set; }

        public IC1 C { get; private set; }
    }
}
