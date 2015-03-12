using Autofac.Features.OwnedInstances;
using Xunit;

namespace Autofac.Test.Features.OwnedInstances
{
    public class OwnedTests
    {
        [Fact]
        public void WhenInitialisedWithValue_ReturnsSameFromValueProperty()
        {
            var value = "Hello";
            var owned = new Owned<string>(value, new DisposeTracker());
            Assert.Same(value, owned.Value);
        }

        [Fact]
        public void DisposingOwned_CallsDisposeOnLifetimeToken()
        {
            var lifetime = new DisposeTracker();
            var owned = new Owned<string>("unused", lifetime);
            owned.Dispose();
            Assert.True(lifetime.IsDisposed);
        }
    }
}
