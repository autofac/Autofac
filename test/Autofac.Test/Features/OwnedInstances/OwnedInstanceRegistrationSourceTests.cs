using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.OwnedInstances;
using Xunit;

namespace Autofac.Test.Features.OwnedInstances
{
    public class OwnedInstanceRegistrationSourceTests
    {
        [Fact]
        public void WhenTIsRegistered_OwnedTCanBeResolved()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<DisposeTracker>();
            var c = cb.Build();
            var owned = c.Resolve<Owned<DisposeTracker>>();
            Assert.NotNull(owned.Value);
        }

        [Fact]
        public void CallingDisposeOnGeneratedOwnedT_DisposesOwnedInstance()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<DisposeTracker>();
            var c = cb.Build();

            var owned = c.Resolve<Owned<DisposeTracker>>();
            var dt = owned.Value;
            Assert.False(dt.IsDisposed);
            owned.Dispose();
            Assert.True(dt.IsDisposed);
        }

        [Fact]
        public void CallingDisposeOnGeneratedOwnedT_DoesNotDisposeCurrentLifetimeScope()
        {
            var cb = new ContainerBuilder();
            var containerDisposeTracker = new DisposeTracker();
            cb.RegisterInstance(containerDisposeTracker).Named<DisposeTracker>("tracker");
            cb.RegisterType<DisposeTracker>();
            var c = cb.Build();

            var owned = c.Resolve<Owned<DisposeTracker>>();
            owned.Dispose();
            Assert.False(containerDisposeTracker.IsDisposed);
        }

        [Fact]
        public void IfInnerTypeIsNotRegistered_OwnedTypeIsNotEither()
        {
            var c = new ContainerBuilder().Build();
            Assert.False(c.IsRegistered<Owned<Object>>());
        }

        [Fact]
        public void ResolvingOwnedInstanceByName_ReturnsValueByName()
        {
            object o = new object();

            var builder = new ContainerBuilder();
            builder.RegisterInstance(o).Named<object>("o");
            var container = builder.Build();

            var owned = container.ResolveNamed<Owned<object>>("o");

            Assert.Same(o, owned.Value);
        }

        public class ExposesScopeTag
        {
            private readonly ILifetimeScope _myScope;

            public ExposesScopeTag(ILifetimeScope myScope)
            {
                _myScope = myScope;
            }

            public object Tag
            {
                get
                {
                    return _myScope.Tag;
                }
            }
        }

        [Fact]
        public void ScopesCreatedForOwnedInstances_AreTaggedWithTheServiceThatIsOwned()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<ExposesScopeTag>();
            var c = cb.Build();

            var est = c.Resolve<Owned<ExposesScopeTag>>();
            Assert.Equal(new TypedService(typeof(ExposesScopeTag)), est.Value.Tag);
        }

        public class ClassWithFactory
        {
            public string Name { get; set; }

            public delegate Owned<ClassWithFactory> OwnedFactory(string name);

            public ClassWithFactory(string name)
            {
                Name = name;
            }
        }

        [Fact]
        public void CanResolveAndUse_OwnedGeneratedFactory()
        {
            var cb = new ContainerBuilder();
            cb.Register((c, p) => new ClassWithFactory(p.Named<string>("name")));
            cb.RegisterGeneratedFactory<ClassWithFactory.OwnedFactory>();
            var container = cb.Build();
            var factory = container.Resolve<ClassWithFactory.OwnedFactory>();
            bool isAccessed;
            using (var owner = factory("test"))
            {
                Assert.Equal("test", owner.Value.Name);
                isAccessed = true;
            }

            Assert.True(isAccessed);
        }
    }
}
