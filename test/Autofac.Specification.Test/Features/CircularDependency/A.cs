using System;
using System.Linq;

namespace Autofac.Specification.Test.Features.CircularDependency
{
    public class A : IA
    {
        public A(IC c)
        {
        }
    }
}
