using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using Autofac.Core;
using Autofac.Features.OpenGenerics;
using Autofac.Builder;
using Autofac.Core.Activators.Reflection;

namespace Autofac.Tests.Features.OpenGenerics
{
    [TestFixture]
    public class OpenGenericActivatorGeneratorTests
    {
        interface I<T> { }

        class A1<T> : DisposeTracker, I<T> { }

        [Test]
        public void GeneratesActivatorAndCorrectServices()
        {
            var g = new OpenGenericActivatorGenerator();

            IInstanceActivator activator;
            IEnumerable<Service> services;

            Assert.IsTrue(
                g.TryGenerateActivator(new TypedService(typeof(I<int>)),
                    new Service[] { new TypedService(typeof(I<>)) },
                    new ReflectionActivatorData(typeof(A1<>)),
                    out activator, out services));

            Assert.AreEqual(typeof(I<int>),
                services.Cast<TypedService>().Single().ServiceType);

            var activatedInstance = activator.ActivateInstance(Container.Empty, Factory.NoParameters);
            Assert.IsInstanceOfType(typeof(A1<int>), activatedInstance);
        }

        //[Test]
        //public void GenericRegistrationsInSubcontextOverrideRootContext()
        //{
        //    var builder = new ContainerBuilder();
        //    builder.RegisterGeneric(typeof(List<>)).As(typeof(ICollection<>)).FactoryScoped();
        //    var container = builder.Build();
        //    var inner = container.BeginLifetimeScope();
        //    var innerBuilder = new ContainerBuilder();
        //    innerBuilder.RegisterGeneric(typeof(LinkedList<>)).As(typeof(ICollection<>)).FactoryScoped();
        //    innerBuilder.Build(inner);

        //    var list = inner.Resolve<ICollection<int>>();
        //    Assert.IsInstanceOfType(typeof(LinkedList<int>), list);
        //}

        //[Test]
        //public void SingletonGenericComponentsResolvedInSubcontextStickToParent()
        //{
        //    var builder = new ContainerBuilder();
        //    builder.RegisterGeneric(typeof(List<>)).As(typeof(ICollection<>));
        //    var container = builder.Build();
        //    var inner = container.BeginLifetimeScope();

        //    var innerList = inner.Resolve<ICollection<int>>();
        //    var outerList = container.Resolve<ICollection<int>>();
        //    Assert.AreSame(innerList, outerList);
        //}
    }
}
