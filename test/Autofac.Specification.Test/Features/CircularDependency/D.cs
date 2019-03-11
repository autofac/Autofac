using System;
using System.Linq;

namespace Autofac.Specification.Test.Features.CircularDependency
{
    public class D : ID
    {
        public D(IA a, IC c)
        {
        }
    }
}
