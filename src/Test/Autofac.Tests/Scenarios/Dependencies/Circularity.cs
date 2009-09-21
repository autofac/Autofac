using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Tests.Scenarios.Dependencies.Circularity
{
    interface IA { }

    interface IB { }

    interface IC { }

    interface ID { }

    class A : IA
    {
        public A(IC c) { }
    }

    class BC : IB, IC
    {
        public BC(IA a) { }
    }

    class D : ID
    {
        public D(IA a, IC c) { }
    }
}
