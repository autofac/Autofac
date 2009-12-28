using System.Collections.Generic;
using Autofac.Builder;
using NUnit.Framework;
using Autofac.Core;
using System;

namespace Autofac.Tests.Builder
{
    public interface IG<T>
    {
    }

    public class G<T> : IG<T>
    {
        public G()
        {
        }

        public G(int i)
        {
            I = i;
        }

        public int I { get; private set; }
    }

    [TestFixture]
    public class GenericRegistrationBuilderFixture
    {
        [Test]
        public void BuildGenericRegistration()
        {
            var componentType = typeof(G<>);
            var serviceType = typeof(IG<>);
            var concreteServiceType = typeof(IG<int>);

            var cb = new ContainerBuilder();
            cb.RegisterGeneric(componentType)
                .As(serviceType);
            var c = cb.Build();

            object g1 = c.Resolve(concreteServiceType);
            object g2 = c.Resolve(concreteServiceType);

            Assert.IsNotNull(g1);
            Assert.IsNotNull(g2);
            Assert.AreNotSame(g1, g2);
            Assert.IsTrue(g1.GetType().GetGenericTypeDefinition() == componentType);
        }

        [Test]
        public void ExposesImplementationType()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(G<>)).As(typeof(IG<>));
            var container = cb.Build();
            IComponentRegistration cr;
            Assert.IsTrue(container.ComponentRegistry.TryGetRegistration(
                new TypedService(typeof(IG<int>)), out cr));
            Assert.AreEqual(typeof(G<int>), cr.Activator.LimitType);
        }

        [Test]
        public void FiresPreparing()
        {
            int preparingFired = 0;
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(G<>))
                .As(typeof(IG<>))
                .OnPreparing(e => ++preparingFired);
            var container = cb.Build();
            container.Resolve<IG<int>>();
            Assert.AreEqual(1, preparingFired);
        }

        [Test]
        public void WhenNoServicesExplicitlySpecified_GenericComponentTypeIsService()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(G<>));
            var c = cb.Build();
            Assertions.AssertRegistered<G<int>>(c);
        }

        [Test]
        public void SuppliesParameterToConcreteComponent()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(G<>)).WithParameter(new NamedParameter("i", 42));
            var c = cb.Build();
            var g = c.Resolve<G<string>>();
            Assert.AreEqual(42, g.I);
        }
    }
}
