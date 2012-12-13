using System;
using System.Linq;
using Autofac.Core;
using Autofac.Integration.Wcf;
using Autofac.Util;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Wcf
{
    [TestFixture]
    public class AutofacInstanceContextFixture
    {
        [Test(Description = "You can't create an instance context without a parent scope.")]
        public void Ctor_RequiresParentScope()
        {
            Assert.Throws<ArgumentNullException>(() => new AutofacInstanceContext(null));
        }

        [Test(Description = "If there is no OperationContext there should be no operation lifetime scope.")]
        public void Current_NoOperationContext()
        {
            Assert.IsNull(AutofacInstanceContext.Current, "There should not have been an instance context.");
        }

        [Test(Description = "When the instance context gets disposed, service instances should also be disposed.")]
        public void Dispose_InstancesDisposed()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<DisposeTracker>();
            var container = builder.Build();

            var impData = new ServiceImplementationData()
            {
                ConstructorString = "TestService",
                ServiceTypeToHost = typeof(DisposeTracker),
                ImplementationResolver = l => l.Resolve<DisposeTracker>()
            };

            var context = new AutofacInstanceContext(container);
            var disposable = (DisposeTracker)context.Resolve(impData);
            Assert.IsFalse(disposable.IsDisposedPublic);
            context.Dispose();
            Assert.IsTrue(disposable.IsDisposedPublic);
        }

        [Test]
        public void Dispose_RegistrationInstancesDisposed()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<DisposeTracker>();
            var container = builder.Build();
            IComponentRegistration registration;
            container.ComponentRegistry.TryGetRegistration(new TypedService(typeof(DisposeTracker)), out registration);
            var context = new AutofacInstanceContext(container);
            var disposable = (DisposeTracker)context.ResolveComponent(registration, Enumerable.Empty<Parameter>());
            Assert.IsFalse(disposable.IsDisposedPublic);
            context.Dispose();
            Assert.IsTrue(disposable.IsDisposedPublic);
        }

        [Test(Description = "You can't resolve a service implementation without the data about the resolution.")]
        public void Resolve_RequiresServiceImplementationData()
        {
            var context = new AutofacInstanceContext(new ContainerBuilder().Build());
            Assert.Throws<ArgumentNullException>(() => context.Resolve(null));
        }

        private class DisposeTracker : Disposable
        {
            public bool IsDisposedPublic
            {
                get
                {
                    return this.IsDisposed;
                }
            }
        }

    }
}
