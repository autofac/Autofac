using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Features.Metadata;
using Autofac.Features.OwnedInstances;
using Autofac.Specification.Test.Lifetime;
using Autofac.Specification.Test.Util;
using Xunit;

namespace Autofac.Specification.Test.Features
{
    public class CompositeTests
    {
        [Fact]
        public void CanRegisterComposite()
        {
            var builder = new ContainerBuilder();
            builder.Register(ctx => new S1()).As<I1>();
            builder.Register(ctx => new S2()).As<I1>();

            builder.RegisterComposite<MyComposite, I1>();

            var container = builder.Build();

            var comp = container.Resolve<I1>();

            Assert.IsType<MyComposite>(comp);

            var actualComp = (MyComposite)comp;

            Assert.Collection(
                actualComp.Implementations,
                i => Assert.IsType<S1>(i),
                i => Assert.IsType<S2>(i));
        }

        [Fact]
        public void CompositeRegistrationOrderIrrelevant()
        {
            var builder = new ContainerBuilder();

            builder.Register(ctx => new S1()).As<I1>();
            builder.RegisterComposite<MyComposite, I1>();
            builder.Register(ctx => new S2()).As<I1>();

            var container = builder.Build();

            var comp = container.Resolve<I1>();

            Assert.IsType<MyComposite>(comp);

            var actualComp = (MyComposite)comp;

            Assert.Collection(
                actualComp.Implementations,
                i => Assert.IsType<S1>(i),
                i => Assert.IsType<S2>(i));
        }

        [Fact]
        public void CanRegisterCompositeWithDirectType()
        {
            var builder = new ContainerBuilder();
            builder.Register(ctx => new S1()).As<I1>();
            builder.Register(ctx => new S2()).As<I1>();

            builder.RegisterComposite(typeof(MyComposite), typeof(I1));

            var container = builder.Build();

            var comp = container.Resolve<I1>();

            Assert.IsType<MyComposite>(comp);

            var actualComp = (MyComposite)comp;

            Assert.Collection(
                actualComp.Implementations,
                i => Assert.IsType<S1>(i),
                i => Assert.IsType<S2>(i));
        }

        [Fact]
        public void CanRegisterCompositeWithDelegate()
        {
            var builder = new ContainerBuilder();
            builder.Register(ctx => new S1()).As<I1>();
            builder.Register(ctx => new S2()).As<I1>();

            builder.RegisterComposite<I1>((ctxt, concrete) => new MyComposite(concrete));

            var container = builder.Build();

            var comp = container.Resolve<I1>();

            Assert.IsType<MyComposite>(comp);

            var actualComp = (MyComposite)comp;

            Assert.Collection(
                actualComp.Implementations,
                i => Assert.IsType<S1>(i),
                i => Assert.IsType<S2>(i));
        }

        [Fact]
        public void CanRegisterGenericComposite()
        {
            var builder = new ContainerBuilder();
            builder.Register(ctx => new GenericImplInt()).As<IGenericService<int>>();
            builder.Register(ctx => new GenericImplInt2()).As<IGenericService<int>>();
            builder.RegisterGeneric(typeof(OpenGenericImpl<>)).As(typeof(IGenericService<>));

            builder.RegisterGenericComposite(typeof(GenericComposite<>), typeof(IGenericService<>));

            var container = builder.Build();

            var comp = container.Resolve<IGenericService<int>>();

            Assert.IsType<GenericComposite<int>>(comp);

            var actualComp = (GenericComposite<int>)comp;

            Assert.Collection(
                actualComp.Implementations,
                i => Assert.IsType<GenericImplInt>(i),
                i => Assert.IsType<GenericImplInt2>(i),
                i => Assert.IsType<OpenGenericImpl<int>>(i));
        }

        [Fact]
        public void CanRegisterCompositeInNestedScope()
        {
            var builder = new ContainerBuilder();
            builder.Register(ctx => new S1()).As<I1>();
            builder.Register(ctx => new S2()).As<I1>();

            var container = builder.Build();

            Assert.IsType<S2>(container.Resolve<I1>());

            var nested = container.BeginLifetimeScope(cfg =>
            {
                cfg.RegisterComposite<MyComposite, I1>();
            });

            var comp = nested.Resolve<I1>();

            Assert.IsType<MyComposite>(comp);

            var actualComp = (MyComposite)comp;

            Assert.Collection(
                actualComp.Implementations,
                i => Assert.IsType<S1>(i),
                i => Assert.IsType<S2>(i));

            // Check that outer scope is still composite-less.
            Assert.IsType<S2>(container.Resolve<I1>());
        }

        [Fact]
        public void CompositeForNamedScopeThrowsExceptionIfServiceResolvedInDifferentScope()
        {
            var builder = new ContainerBuilder();
            builder.Register(ctx => new S1()).As<I1>();
            builder.Register(ctx => new S2()).As<I1>();

            var activatedCount = 0;

            builder.RegisterComposite<MyComposite, I1>()
                   .InstancePerMatchingLifetimeScope("tag1")
                   .OnActivated(args => activatedCount++);

            var container = builder.Build();

            var nested = container.BeginLifetimeScope();

            Assert.Throws<DependencyResolutionException>(() => nested.Resolve<I1>());
        }

        [Fact]
        public void CompositeForNamedScopeResolvedInNestedScope()
        {
            var builder = new ContainerBuilder();
            builder.Register(ctx => new S1()).As<I1>();
            builder.Register(ctx => new S2()).As<I1>();

            var activatedCount = 0;

            builder.RegisterComposite<MyComposite, I1>()
                   .InstancePerMatchingLifetimeScope("tag1")
                   .OnActivated(args => activatedCount++);

            var container = builder.Build();

            var nested = container.BeginLifetimeScope("tag1").BeginLifetimeScope(cfg => { });

            Assert.IsType<MyComposite>(nested.Resolve<I1>());
        }

        [Fact]
        public void AdditionalRegistrationsInNestedScopeAreIncludedInComposite()
        {
            var builder = new ContainerBuilder();
            builder.Register(ctx => new S1()).As<I1>();
            builder.Register(ctx => new S2()).As<I1>();

            builder.RegisterComposite<MyComposite, I1>();

            var container = builder.Build();

            var nested = container.BeginLifetimeScope(cfg =>
            {
                cfg.Register(ctx => new S3()).As<I1>();
            });

            var comp = nested.Resolve<I1>();

            Assert.IsType<MyComposite>(comp);

            var actualComp = (MyComposite)comp;

            Assert.Collection(
                actualComp.Implementations,
                i => Assert.IsType<S1>(i),
                i => Assert.IsType<S2>(i),
                i => Assert.IsType<S3>(i));
        }

        [Fact]
        public void CompositeWithCircularDependencyThrows()
        {
            var builder = new ContainerBuilder();
            builder.Register(ctx => new S1()).As<I1>();
            builder.Register(ctx => new S2()).As<I1>();

            builder.RegisterComposite<CircularComposite, I1>();

            var container = builder.Build();

            Assert.Throws<DependencyResolutionException>(() => container.Resolve<I1>());
        }

        [Fact]
        public void CompositeCanAccessMetaOfRegistrations()
        {
            var builder = new ContainerBuilder();
            builder.Register(ctx => new S1()).As<I1>();
            builder.Register(ctx => new S2()).As<I1>();

            builder.RegisterComposite<MyComposite<Meta<I1>>, I1>();

            var container = builder.Build();

            var comp = container.Resolve<I1>();

            Assert.IsType<MyComposite<Meta<I1>>>(comp);

            var actualComp = (MyComposite<Meta<I1>>)comp;

            Assert.Collection(
                actualComp.Implementations,
                i => Assert.IsType<S1>(i.Value),
                i => Assert.IsType<S2>(i.Value));
        }

        [Fact]
        public void CompositeCanNestAdaptersOnResolvedImplementations()
        {
            var builder = new ContainerBuilder();
            builder.Register(ctx => new S1()).As<I1>();
            builder.Register(ctx => new S2()).As<I1>();

            builder.RegisterComposite<MyComposite<Func<Meta<I1>>>, I1>();

            var container = builder.Build();

            var comp = container.Resolve<I1>();

            Assert.IsType<MyComposite<Func<Meta<I1>>>>(comp);

            var actualComp = (MyComposite<Func<Meta<I1>>>)comp;

            Assert.Collection(
                actualComp.Implementations,
                i => Assert.IsType<S1>(i().Value),
                i => Assert.IsType<S2>(i().Value));
        }

        [Fact]
        public void CompositeCanHaveOwnMeta()
        {
            var builder = new ContainerBuilder();
            builder.Register(ctx => new S1()).As<I1>();
            builder.Register(ctx => new S2()).As<I1>();

            builder.RegisterComposite<MyComposite, I1>()
                   .WithMetadata("data", "value");

            var container = builder.Build();

            var comp = container.Resolve<Meta<I1>>();

            Assert.IsType<MyComposite>(comp.Value);

            Assert.Equal("value", comp.Metadata["data"]);
        }

        [Fact]
        public void CompositeCanBeResolveByFunc()
        {
            var builder = new ContainerBuilder();
            builder.Register(ctx => new S1()).As<I1>();
            builder.Register(ctx => new S2()).As<I1>();

            builder.RegisterComposite<MyComposite, I1>();

            var container = builder.Build();

            var comp = container.Resolve<Func<I1>>();

            Assert.IsType<MyComposite>(comp());
        }

        [Fact]
        public void CompositeCanBeResolvedAsOwned()
        {
            var builder = new ContainerBuilder();
            builder.Register(ctx => new S1()).As<I1>();
            builder.Register(ctx => new S2()).As<I1>();

            builder.RegisterComposite<MyComposite, I1>();

            var container = builder.Build();

            var comp = container.Resolve<Owned<I1>>();

            Assert.IsType<MyComposite>(comp.Value);
        }

        [Fact]
        public void CompositeAsOwnedDependenciesDisposed()
        {
            var builder = new ContainerBuilder();
            builder.Register(ctx => new S1()).As<I1>();
            builder.Register(ctx => new S2()).As<I1>();

            var didDisposeTracker = 0;

            builder.RegisterType<DisposeTracker>().OnActivated(args => args.Instance.Disposing += (s, e) => didDisposeTracker++);

            builder.RegisterComposite<MyCompositeNeedsDisposeTracker, I1>();

            var container = builder.Build();

            var comp = container.Resolve<Owned<I1>>();

            comp.Dispose();

            Assert.Equal(1, didDisposeTracker);
        }

        [Fact]
        public void CompositeCanLazyResolveIndividualRegistrations()
        {
            var builder = new ContainerBuilder();
            builder.Register(ctx => new S1()).As<I1>();
            builder.Register(ctx => new S2()).As<I1>();

            builder.RegisterComposite<MyLazyComposite, I1>();

            var container = builder.Build();

            var comp = (MyLazyComposite)container.Resolve<I1>();

            Assert.Equal(2, comp.Implementations.Value.Count());
        }

        [Fact]
        public void CompositeCanHaveOwnLifetime()
        {
            var builder = new ContainerBuilder();
            builder.Register(ctx => new S1()).As<I1>();
            builder.Register(ctx => new S2()).As<I1>();

            var activatedCount = 0;

            builder.RegisterComposite<MyComposite, I1>()
                   .OnActivated(args =>
                   {
                       activatedCount++;
                   })
                   .SingleInstance();

            var container = builder.Build();

            var comp = container.Resolve<I1>();

            Assert.IsType<MyComposite>(comp);

            comp = container.Resolve<I1>();

            Assert.Equal(1, activatedCount);
        }

        [Fact]
        public void CompositeWithSingletonLifetimeIgnoresRegistrationsInChildScopes()
        {
            var builder = new ContainerBuilder();
            builder.Register(ctx => new S1()).As<I1>();
            builder.Register(ctx => new S2()).As<I1>();

            builder.RegisterComposite<MyComposite, I1>()
                   .SingleInstance();

            var container = builder.Build();

            var nested = container.BeginLifetimeScope(cfg => cfg.Register(ctxt => new S3()).As<I1>());

            var comp = (MyComposite)nested.Resolve<I1>();

            Assert.Equal(2, comp.Implementations.Count);
        }

        [Fact]
        public void CompositeCannotBeCompositeForMultipleServices()
        {
            var builder = new ContainerBuilder();
            builder.Register(ctx => new S1()).As<I1>();
            builder.Register(ctx => new S2()).As<I1>();

            builder.RegisterComposite<MultiComposite, I1>().As<I2>();

            Assert.Throws<InvalidOperationException>(() => builder.Build());
        }

        [Fact]
        public void CompositeIsNotDecoratedButConcreteImplementationsAre()
        {
            var builder = new ContainerBuilder();
            builder.Register(ctx => new S1()).As<I1>();
            builder.Register(ctx => new S2()).As<I1>();
            builder.RegisterDecorator<DecoratorForI1, I1>();

            builder.RegisterComposite<I1>((ctxt, concrete) => new MyComposite(concrete));

            var container = builder.Build();

            var comp = container.Resolve<I1>();

            Assert.IsType<MyComposite>(comp);

            var actualComp = (MyComposite)comp;

            Assert.Collection(
                actualComp.Implementations,
                i => Assert.IsType<S1>(Assert.IsType<DecoratorForI1>(i).Instance),
                i => Assert.IsType<S2>(Assert.IsType<DecoratorForI1>(i).Instance));
        }

        [Fact]
        public void SecondCompositeForSameServiceOverridesFirst()
        {
            var builder = new ContainerBuilder();
            builder.Register(ctx => new S1()).As<I1>();
            builder.Register(ctx => new S2()).As<I1>();

            builder.RegisterComposite<MyComposite, I1>();

            builder.RegisterComposite<MyCompositeTheSecond, I1>();

            var container = builder.Build();

            var comp = container.Resolve<I1>();

            Assert.IsType<MyCompositeTheSecond>(comp);

            var actualComp = (MyCompositeTheSecond)comp;

            Assert.Collection(
                actualComp.Implementations,
                i => Assert.IsType<S1>(i),
                i => Assert.IsType<S2>(i));
        }

        [Fact]
        public void SecondCompositeForSameServiceInNestedScopeOverridesFirst()
        {
            var builder = new ContainerBuilder();
            builder.Register(ctx => new S1()).As<I1>();
            builder.Register(ctx => new S2()).As<I1>();

            builder.RegisterComposite<MyComposite, I1>();

            var container = builder.Build();

            var nested = container.BeginLifetimeScope(cfg => cfg.RegisterComposite<MyCompositeTheSecond, I1>());

            var comp = nested.Resolve<I1>();

            Assert.IsType<MyCompositeTheSecond>(comp);

            var actualComp = (MyCompositeTheSecond)comp;

            Assert.Collection(
                actualComp.Implementations,
                i => Assert.IsType<S1>(i),
                i => Assert.IsType<S2>(i));
        }

        [Fact]
        public void RegistrationSourceCanProvideComposite()
        {
            var builder = new ContainerBuilder();
            builder.Register(ctx => new S1()).As<I1>();
            builder.Register(ctx => new S2()).As<I1>();
            builder.RegisterSource(new CompositeSupplierSource());

            var container = builder.Build();

            var comp = container.Resolve<I1>();

            Assert.IsType<MyComposite>(comp);

            var actualComp = (MyComposite)comp;

            Assert.Collection(
                actualComp.Implementations,
                i => Assert.IsType<S1>(i),
                i => Assert.IsType<S2>(i));
        }

        [Fact]
        public void RegistrationSourceCanProvideCompositeInNestedScope()
        {
            var builder = new ContainerBuilder();
            builder.Register(ctx => new S1()).As<I1>();
            builder.Register(ctx => new S2()).As<I1>();
            builder.RegisterSource(new CompositeSupplierSource());

            var container = builder.Build();

            var comp = container.BeginLifetimeScope(cfg => { }).Resolve<I1>();

            Assert.IsType<MyComposite>(comp);

            var actualComp = (MyComposite)comp;

            Assert.Collection(
                actualComp.Implementations,
                i => Assert.IsType<S1>(i),
                i => Assert.IsType<S2>(i));
        }

        [Fact]
        public void ManualCompositeOverridesSourcedComposite()
        {
            var builder = new ContainerBuilder();
            builder.Register(ctx => new S1()).As<I1>();
            builder.Register(ctx => new S2()).As<I1>();
            builder.RegisterSource(new CompositeSupplierSource());

            builder.RegisterComposite<MyCompositeTheSecond, I1>();

            var container = builder.Build();

            var comp = container.Resolve<I1>();

            Assert.IsType<MyCompositeTheSecond>(comp);
        }

        [Fact]
        public void SourceInNestedScopeOverridesManualComposite()
        {
            var builder = new ContainerBuilder();
            builder.Register(ctx => new S1()).As<I1>();
            builder.Register(ctx => new S2()).As<I1>();
            builder.RegisterComposite<MyCompositeTheSecond, I1>();

            var container = builder.Build();

            var comp = container.BeginLifetimeScope(cfg => cfg.RegisterSource(new CompositeSupplierSource())).Resolve<I1>();

            Assert.IsType<MyComposite>(comp);
        }

        private class CompositeSupplierSource : IRegistrationSource
        {
            public bool IsAdapterForIndividualComponents => true;

            public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
            {
                if (service is IServiceWithType swt && swt.ServiceType == typeof(I1))
                {
                    var rb = RegistrationBuilder.ForType<MyComposite>().As<I1>();

                    rb.RegistrationData.Options |= RegistrationOptions.Composite;

                    yield return rb.CreateRegistration();
                }
            }
        }

        private class MyComposite : MyComposite<I1>, I1
        {
            public MyComposite(IEnumerable<I1> implementations)
                : base(implementations.ToList())
            {
            }
        }

        private class MyCompositeNeedsDisposeTracker : MyComposite<I1>, I1
        {
            public MyCompositeNeedsDisposeTracker(DisposeTracker tracker, IEnumerable<I1> implementations)
                : base(implementations.ToList())
            {
            }
        }

        private class MyCompositeTheSecond : MyComposite<I1>
        {
            public MyCompositeTheSecond(IEnumerable<I1> implementations)
                   : base(implementations.ToList())
            {
            }
        }

        private class MyComposite<TSetElement> : I1
        {
            public MyComposite(IList<TSetElement> implementations)
            {
                Implementations = implementations;
            }

            public IList<TSetElement> Implementations { get; }
        }

        private class MultiComposite : I1, I2
        {
            public MultiComposite(IEnumerable<I1> composite1, IEnumerable<I2> composite2)
            {
                Composite1 = composite1;
                Composite2 = composite2;
            }

            public IEnumerable<I1> Composite1 { get; }

            public IEnumerable<I2> Composite2 { get; }
        }

        private class MyLazyComposite : I1
        {
            public MyLazyComposite(Lazy<IEnumerable<I1>> implementations)
            {
                Implementations = implementations;
            }

            public Lazy<IEnumerable<I1>> Implementations { get; }
        }

        private class CircularComposite : I1
        {
            public CircularComposite(I1 circular)
            {
            }
        }

        private class DecoratorForI1 : I1
        {
            public DecoratorForI1(I1 instance)
            {
                Instance = instance;
            }

            public I1 Instance { get; }
        }

        private interface IGenericService<TValue>
        {
        }

        private class GenericImplInt : IGenericService<int>
        {
        }

        private class GenericImplInt2 : IGenericService<int>
        {
        }

        private class OpenGenericImpl<TValue> : IGenericService<TValue>
        {
        }

        private class GenericComposite<TValue> : IGenericService<TValue>
        {
            public GenericComposite(IList<IGenericService<TValue>> implementations)
            {
                Implementations = implementations;
            }

            public IList<IGenericService<TValue>> Implementations { get; }
        }

        private interface I1
        {
        }

        private interface I2
        {
        }

        private class S1 : I1
        {
        }

        private class S2 : I1
        {
        }

        private class S3 : I1
        {
        }

        private class S4 : I1
        {
        }
    }
}
