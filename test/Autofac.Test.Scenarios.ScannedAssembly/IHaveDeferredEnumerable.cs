using System.Collections.Generic;

namespace Autofac.Test.Scenarios.ScannedAssembly
{
    public interface IHaveDeferredEnumerable
    {
        IEnumerable<IHaveDeferredEnumerable> Get();
    }
}
