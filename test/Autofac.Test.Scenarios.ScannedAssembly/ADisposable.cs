using System;

namespace Autofac.Test.Scenarios.ScannedAssembly
{
    public class ADisposable : IDisposable
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
