using System;

namespace AutofacContrib.Tests.Multitenant.Stubs
{
    public class StubDisposableDependency : IDisposable
    {
        // Intentionally a simple (and incorrect) disposable implementation.
        // We need it for testing if Dispose was called, not actually to do
        // the standard Dispose cleanup.

        public bool IsDisposed = false;

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
