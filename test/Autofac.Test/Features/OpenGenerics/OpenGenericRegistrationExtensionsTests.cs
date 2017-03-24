using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core;
using Autofac.Features.OpenGenerics;
using Xunit;

namespace Autofac.Test.Features.OpenGenerics
{
    public class OpenGenericRegistrationExtensionsTests
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

        [Fact]
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

            Assert.NotNull(g1);
            Assert.NotNull(g2);
            Assert.NotSame(g1, g2);
            Assert.True(g1.GetType().GetGenericTypeDefinition() == componentType);
        }

        [Fact]
        public void ExposesImplementationType()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(G<>)).As(typeof(IG<>));
            var container = cb.Build();
            IComponentRegistration cr;
            Assert.True(container.ComponentRegistry.TryGetRegistration(
                new TypedService(typeof(IG<int>)), out cr));
            Assert.Equal(typeof(G<int>), cr.Activator.LimitType);
        }

        [Fact]
        public void FiresPreparing()
        {
            int preparingFired = 0;
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(G<>))
                .As(typeof(IG<>))
                .OnPreparing(e => ++preparingFired);
            var container = cb.Build();
            container.Resolve<IG<int>>();
            Assert.Equal(1, preparingFired);
        }

        [Fact]
        public void WhenNoServicesExplicitlySpecifiedGenericComponentTypeIsService()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(G<>));
            var c = cb.Build();
            c.AssertRegistered<G<int>>();
        }

        [Fact]
        public void SuppliesParameterToConcreteComponent()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(G<>)).WithParameter(new NamedParameter("i", 42));
            var c = cb.Build();
            var g = c.Resolve<G<string>>();
            Assert.Equal(42, g.I);
        }

        [Fact]
        public void WhenRegistrationNamedGenericRegistrationsSuppliedViaName()
        {
            const string name = "n";
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(G<>))
                .Named(name, typeof(IG<>));
            var c = cb.Build();
            Assert.True(c.IsRegisteredWithName<IG<int>>(name));
            Assert.True(c.IsRegisteredWithName<IG<string>>(name));
        }

        [Fact]
        public void RegisterGenericRejectsNonOpenGenericTypes()
        {
            var cb = new ContainerBuilder();
            Assert.Throws<ArgumentException>(() => cb.RegisterGeneric(typeof(List<int>)));
        }

        public interface ITwoParams<T, TU>
        {
        }

        public class TwoParams<T, TU> : ITwoParams<T, TU>
        {
        }

        [Fact]
        public void MultipleTypeParametersAreMatched()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(TwoParams<,>)).As(typeof(ITwoParams<,>));
            var c = cb.Build();
            c.Resolve<ITwoParams<int, string>>();
        }

        [Fact]
        public void NonGenericServiceTypesAreRejected()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(IList<>)).As(typeof(object));
            Assert.Throws<ArgumentException>(() =>
            {
                cb.Build();
            });
        }

        [Fact]
        public void WhenAnOpenGenericIsRegisteredWithANameItProvidesOnlyOneImplementation()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(G<>)).Named("n", typeof(IG<>));
            var c = cb.Build();
            Assert.Equal(1, c.ResolveNamed<IEnumerable<IG<int>>>("n").Count());
        }

        [Fact]
        public void WhenAnOpenGenericIsRegisteredWithANameItCannotBeResolvedWithoutOne()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(G<>)).Named("n", typeof(IG<>));
            var c = cb.Build();
            Assert.Equal(0, c.Resolve<IEnumerable<IG<int>>>().Count());
        }

        [Fact]
        public void WhenAnOpenGenericIsRegisteredItProvidesOnlyOneImplementation()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(G<>)).As(typeof(IG<>));
            var c = cb.Build();
            Assert.Equal(1, c.Resolve<IEnumerable<IG<int>>>().Count());
        }

        public class FG<T>
        {
        }

        [Fact]
        public void WhenAnOpenGenericIsRegisteredAndItProvidesNoImplementationItShouldHaveAGoodError()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(FG<>)).As(typeof(IG<>));

            var exception = Assert.Throws<InvalidOperationException>(() => cb.Build());

            var message = string.Format(OpenGenericServiceBinderResources.ImplementorDoesntImplementService, typeof(FG<>).FullName, typeof(IG<>).FullName);
            Assert.Equal(message, exception.Message);
        }
    }
}
