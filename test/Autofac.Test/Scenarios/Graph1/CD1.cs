using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Test.Scenarios.Graph1
{
    // In the below scenario, B1 depends on A1, CD depends on A1 and B1,
    // and E depends on IC1 and B1.
    public class CD1 : DisposeTracker, IC1, ID1
    {
        public A1 A { get; private set; }

        public B1 B { get; private set; }

        public CD1(A1 a, B1 b)
        {
            A = a;
            B = b;
        }
    }
}