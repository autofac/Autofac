using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Tests.Scenarios.Dependencies.Circularity
{
    public interface IA { }

    public interface IB { }

    public interface IC { }

    public interface ID { }

    public class A : IA
    {
        public A(IC c) { }
    }

    public class BC : IB, IC
    {
        public BC(IA a) { }
    }

    public class D : ID
    {
        public D(IA a, IC c) { }
    }
}
