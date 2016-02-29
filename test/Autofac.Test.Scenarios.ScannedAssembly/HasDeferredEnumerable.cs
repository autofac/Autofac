using System.Collections.Generic;

namespace Autofac.Test.Scenarios.ScannedAssembly
{
    public class HasDeferredEnumerable : IHaveDeferredEnumerable
    {
        public IEnumerable<IHaveDeferredEnumerable> Get()
        {
            yield return null;
        }
    }
}
