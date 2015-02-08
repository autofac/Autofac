#if !ASPNETCORE50
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Autofac.Features.OwnedInstances;
using Moq;

namespace Autofac.Test.Features.OwnedInstances
{
    public class OwnedTests
    {
        [Fact]
        public void WhenInitialisedWithValue_ReturnsSameFromValueProperty()
        {
            var value = "Hello";
            var owned = new Owned<string>(value, new Mock<IDisposable>().Object);
            Assert.Same(value, owned.Value);
        }

        [Fact]
        public void DisposingOwned_CallsDisposeOnLifetimeToken()
        {
            var lifetime = new Mock<IDisposable>();
            lifetime.Setup(l => l.Dispose()).Verifiable();
            var owned = new Owned<string>("unused", lifetime.Object);
            owned.Dispose();
            lifetime.VerifyAll();
        }
    }
}
#endif
