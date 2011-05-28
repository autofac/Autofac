using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Tests.Scenarios.Graph1
{
    // In the below scenario, B1 depends on A1, CD depends on A1 and B1,
    // and E depends on IC1 and B1.

    public class A1 : DisposeTracker { }

    public class B1 : DisposeTracker
    {
        public A1 A;

        public B1(A1 a)
        {
            A = a;
        }
    }

    public interface IC1 { }

    public class C1 : DisposeTracker
    {
        public B1 B;

        public C1(B1 b)
        {
            B = b;
        }
    }

    public interface ID1 { }

    public class CD1 : DisposeTracker, IC1, ID1
    {
        public A1 A;
        public B1 B;

        public CD1(A1 a, B1 b)
        {
            A = a;
            B = b;
        }
    }

    public class E1 : DisposeTracker
    {
        public B1 B;
        public IC1 C;

        public E1(B1 b, IC1 c)
        {
            B = b;
            C = c;
        }
    }

    public class F1
    {
        public IList<A1> AList;
        public F1(IList<A1> aList)
        {
            AList = aList;
        }
    }
}
