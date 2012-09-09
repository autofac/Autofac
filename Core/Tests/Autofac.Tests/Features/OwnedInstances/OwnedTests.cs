using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac.Features.OwnedInstances;
using Moq;

namespace Autofac.Tests.Features.OwnedInstances
{
    [TestFixture]
    public class OwnedTests
    {
        [Test]
        public void WhenInitialisedWithValue_ReturnsSameFromValueProperty()
        {
            var value = "Hello";
            var owned = new Owned<string>(value, new Mock<IDisposable>().Object);
            Assert.AreSame(value, owned.Value);
        }

        [Test]
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
