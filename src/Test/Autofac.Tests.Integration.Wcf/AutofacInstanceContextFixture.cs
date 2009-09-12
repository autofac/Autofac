using Autofac.Builder;
using Autofac.Integration.Wcf;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Wcf
{
    [TestFixture]
    public class AutofacInstanceContextFixture
    {
        class DisposeTracker : Disposable
        {
            new public bool IsDisposed
            {
                get
                {
                    return base.IsDisposed;
                }
            }
        }

        [Test]
        public void InstancesDisposed()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<DisposeTracker>();
            var container = builder.Build();
            var context = new AutofacInstanceContext(container);
            var disposable = (DisposeTracker)context.Resolve(
                new TypedService(typeof(DisposeTracker)));
            Assert.IsFalse(disposable.IsDisposed);
            context.Dispose();
            Assert.IsTrue(disposable.IsDisposed);
        }
    }
}
