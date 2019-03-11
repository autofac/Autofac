using System;
using System.Linq;

namespace Autofac.Specification.Test.Features.CircularDependency
{
    public class BC : IB, IC
    {
        public BC(IA a)
        {
        }
    }
}
