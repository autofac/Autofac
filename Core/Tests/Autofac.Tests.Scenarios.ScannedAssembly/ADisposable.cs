using System;

namespace Autofac.Tests.Scenarios.ScannedAssembly
{
    public class ADisposable : IDisposable
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
