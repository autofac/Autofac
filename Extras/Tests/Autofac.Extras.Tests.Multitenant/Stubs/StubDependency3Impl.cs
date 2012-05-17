using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Extras.Tests.Multitenant.Stubs
{
    public class StubDependency3Impl : IStubDependency3
    {
        public StubDependency3Impl(IStubDependency1 depends)
        {
            Dependency = depends;
        }

        public IStubDependency1 Dependency { get; private set; }
    }
}
