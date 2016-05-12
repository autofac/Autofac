using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Test.Scenarios.Graph1
{
    // In the below scenario, B1 depends on A1, CD depends on A1 and B1,
    // and E depends on IC1 and B1.
    public class E1 : DisposeTracker
    {
        public B1 B { get; private set; }

        public IC1 C { get; private set; }

        public E1(B1 b, IC1 c)
        {
            B = b;
            C = c;
        }
    }
}