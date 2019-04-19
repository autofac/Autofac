using System;
using System.Linq;
using Autofac.Test.Util;

namespace Autofac.Test.Scenarios.Graph1
{
    // In the below scenario, B1 depends on A1, CD depends on A1 and B1,
    // and E depends on IC1 and B1.
    public class C1 : DisposeTracker
    {
        public C1(B1 b)
        {
            this.B = b;
        }

        public B1 B { get; private set; }
    }
}