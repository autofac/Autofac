using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Extras.Tests.Multitenant.Stubs
{
    /// <summary>
    /// Has a dependency on <see cref="IStubDependency1"/>
    /// </summary>
    public interface IStubDependency3
    {
        IStubDependency1 Dependency { get;}
    }
}
