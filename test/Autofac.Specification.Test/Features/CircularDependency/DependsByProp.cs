using System;
using System.Linq;

namespace Autofac.Specification.Test.Features.CircularDependency
{
    public class DependsByProp
    {
        public DependsByCtor Dep { get; set; }
    }
}
