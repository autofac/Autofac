// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Resolving.Pipeline;
using Autofac.Specification.Test.Registration.Adapters;
using Autofac.Test.Scenarios.ScannedAssembly;
using Xunit;

namespace Autofac.Specification.Test.Registration
{
    public class RegistrationOnlyIfTests
    {
        public delegate object SimpleFactory();

        public interface IService
        {
        }

        public interface IService<T>
        {
        }

        [Fact]
        public void IfNotRegistered_CanFilterEnumerableServices()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ServiceA>().As<IService>();
            builder.RegisterType<ServiceB>().As<IService>().IfNotRegistered(typeof(IService));
            builder.RegisterType<ServiceC>().As<IService>();
            var container = builder.Build();
            var result = container.Resolve<IEnumerable<IService>>().ToArray();
            Assert.Equal(2, result.Length);
            Assert.Contains(result, r => r.GetType() == typeof(ServiceA));
            Assert.Contains(result, r => r.GetType() == typeof(ServiceC));
        }

        [Fact]
        public void IfNotRegistered_CanFilterSingleServices()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ServiceA>().As<IService>();
            builder.RegisterType<ServiceB>().As<IService>().IfNotRegistered(typeof(IService));
            var container = builder.Build();
            Assert.IsType<ServiceA>(container.Resolve<IService>());
        }

        [Fact]
        public void IfNotRegistered_CanFilterMultipleTimes()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ServiceA>().As<IService>().IfNotRegistered(typeof(IService));
            builder.RegisterType<ServiceB>().As<IService>().IfNotRegistered(typeof(IService));
            var container = builder.Build();
            Assert.IsType<ServiceA>(container.Resolve<IService>());
        }

        [Fact]
        public void IfNotRegistered_CanFilterAndRegisterMultipleServices()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ServiceMulti>().As<IService>().As<IService2>().IfNotRegistered(typeof(IService));
            var container = builder.Build();
            Assert.IsType<ServiceMulti>(container.Resolve<IService>());
            Assert.IsType<ServiceMulti>(container.Resolve<IService2>());
        }

        [Fact]
        public void IfNotRegistered_CanBeDecorated()
        {
            // Order here is important - the IfNotRegistered needs to happen
            // before the decorator.
            var builder = new ContainerBuilder();
            builder.RegisterType<ServiceA>().As<IService>().IfNotRegistered(typeof(IService));
            builder.RegisterDecorator<Decorator, IService>();
            var container = builder.Build();
            var result = container.Resolve<IService>();
            Assert.IsType<Decorator>(result);
        }

        [Fact]
        public void IfNotRegistered_CanConditionGenericService()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(SimpleGeneric<>)).IfNotRegistered(typeof(SimpleGeneric<int>));
            var container = builder.Build();

            var result = container.Resolve<SimpleGeneric<int>>();
            Assert.IsType<SimpleGeneric<int>>(result);
        }

        [Fact]
        public void IfNotRegistered_CanConditionGenericServiceWithMultipleInterfaces()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(MultiServiceGeneric<>)).As(typeof(IService<>)).As(typeof(IService2<>));
            builder.RegisterGeneric(typeof(MultiServiceGeneric2<>)).As(typeof(IService<>)).IfNotRegistered(typeof(IService2<int>));
            var container = builder.Build();

            var result = container.Resolve<IService<int>>();
            Assert.IsType<MultiServiceGeneric<int>>(result);

            var result2 = container.Resolve<IService2<int>>();
            Assert.IsType<MultiServiceGeneric<int>>(result2);
        }

        [Fact]
        public void IfNotRegistered_CanHaveServiceMiddleware()
        {
            var builder = new ContainerBuilder();
            var middlewareInvoked = false;

            builder.RegisterType<ServiceA>().As<IService>().IfNotRegistered(typeof(IService));

            builder.RegisterServiceMiddleware<IService>(PipelinePhase.ResolveRequestStart, (ctxt, next) =>
            {
                next(ctxt);
                middlewareInvoked = true;
            });

            var container = builder.Build();
            var result = container.Resolve<IService>();
            Assert.True(middlewareInvoked);
        }

        [Fact]
        public void IfNotRegistered_CanBeDecoratedByModuleWhenModuleRegistered1st()
        {
            var builder = new ContainerBuilder();

            var module = new MyModule(build =>
            {
                build.RegisterDecorator<Decorator, IService>();
            });

            builder.RegisterModule(module);
            builder.RegisterType<ServiceA>().As<IService>().IfNotRegistered(typeof(IService));

            var container = builder.Build();
            var result = container.Resolve<IService>();
            Assert.IsType<Decorator>(result);
        }

        [Fact]
        public void IfNotRegistered_CanBeDecoratedByModuleWhenModuleRegistered2nd()
        {
            var builder = new ContainerBuilder();

            var module = new MyModule(build =>
            {
                build.RegisterDecorator<Decorator, IService>();
            });

            builder.RegisterType<ServiceA>().As<IService>().IfNotRegistered(typeof(IService));
            builder.RegisterModule(module);

            var container = builder.Build();
            var result = container.Resolve<IService>();
            Assert.IsType<Decorator>(result);
        }

        [Fact]
        public void IfNotRegistered_ConditionalInModuleIsDecoratedWhenModuleRegistered1st()
        {
            var builder = new ContainerBuilder();

            var module = new MyModule(build =>
            {
                build.RegisterType<ServiceA>().As<IService>().IfNotRegistered(typeof(IService));
            });

            builder.RegisterModule(module);
            builder.RegisterDecorator<Decorator, IService>();

            var container = builder.Build();
            var result = container.Resolve<IService>();
            Assert.IsType<Decorator>(result);
        }

        [Fact]
        public void IfNotRegistered_ConditionalInModuleIsDecoratedWhenModuleRegistered2nd()
        {
            var builder = new ContainerBuilder();

            var module = new MyModule(build =>
            {
                build.RegisterType<ServiceA>().As<IService>().IfNotRegistered(typeof(IService));
            });

            builder.RegisterDecorator<Decorator, IService>();
            builder.RegisterModule(module);

            var container = builder.Build();
            var result = container.Resolve<IService>();
            Assert.IsType<Decorator>(result);
        }

        [Fact]
        public void OnlyIf_CanFilterEnumerableServices()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ServiceA>().As<IService>();
            builder.RegisterType<ServiceB>().As<IService>().OnlyIf(reg => !reg.IsRegistered(new TypedService(typeof(IService))));
            builder.RegisterType<ServiceC>().As<IService>();
            var container = builder.Build();
            var result = container.Resolve<IEnumerable<IService>>().ToArray();
            Assert.Equal(2, result.Length);
            Assert.Contains(result, r => r.GetType() == typeof(ServiceA));
            Assert.Contains(result, r => r.GetType() == typeof(ServiceC));
        }

        [Fact]
        public void OnlyIf_CanFilterSingleServices()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ServiceA>().As<IService>();
            builder.RegisterType<ServiceB>().As<IService>().OnlyIf(reg => !reg.IsRegistered(new TypedService(typeof(IService))));
            var container = builder.Build();
            Assert.IsType<ServiceA>(container.Resolve<IService>());
        }

        [Fact]
        public void OnlyIf_EnabledByStandardRegistrations()
        {
            // This shouldn't throw during any of the calls. If it does
            // it means the registration extension hasn't been updated
            // to store the callback container with the registration builder.
            var builder = new ContainerBuilder();

            builder.Register((ctx, p) => new object()).OnlyIf(r => true);
            builder.Register(ctx => new object()).OnlyIf(r => true);
            builder.RegisterAdapter<Command, ToolbarButton>((ctx, cmd) => new ToolbarButton(cmd)).As<IToolbarButton>().OnlyIf(r => true);
            builder.RegisterAdapter<Command, ToolbarButton>((ctx, p, cmd) => new ToolbarButton(cmd)).As<IToolbarButton>().OnlyIf(r => true);
            builder.RegisterAdapter<Command, ToolbarButton>(cmd => new ToolbarButton(cmd)).As<IToolbarButton>().OnlyIf(r => true);
            builder.RegisterAssemblyTypes(typeof(AComponent).GetTypeInfo().Assembly).OnlyIf(r => true);
            builder.RegisterDecorator<IService>((ctx, p, s) => new Decorator(s), "from").OnlyIf(r => true);
            builder.RegisterDecorator<IService>((ctx, s) => new Decorator(s), "from").OnlyIf(r => true);
            builder.RegisterDecorator<IService>(s => new Decorator(s), "from").OnlyIf(r => true);
            builder.RegisterGeneratedFactory(typeof(SimpleFactory)).OnlyIf(r => true);
            builder.RegisterGeneratedFactory(typeof(SimpleFactory), new TypedService(typeof(object))).OnlyIf(r => true);
            builder.RegisterGeneratedFactory<SimpleFactory>().OnlyIf(r => true);
            builder.RegisterGeneratedFactory<SimpleFactory>(new TypedService(typeof(object))).OnlyIf(r => true);
            builder.RegisterGeneric(typeof(SimpleGeneric<>)).OnlyIf(r => true);
            builder.RegisterGenericDecorator(typeof(Decorator<>), typeof(IService<>), fromKey: "b").OnlyIf(r => true);
            builder.RegisterInstance(new object()).OnlyIf(r => true);
            builder.RegisterType(typeof(object)).OnlyIf(r => true);
            builder.RegisterType<object>().OnlyIf(r => true);
            builder.RegisterTypes(typeof(object)).OnlyIf(r => true);
        }

        [Fact]
        public void OnlyIf_NullPredicate()
        {
            var builder = new ContainerBuilder();
            var rb = builder.RegisterType<object>();
            Assert.Throws<ArgumentNullException>(() => rb.OnlyIf(null));
        }

        [Fact]
        public void OnlyIf_NullRegistration()
        {
            Assert.Throws<ArgumentNullException>(() => RegistrationExtensions.OnlyIf<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>(null, reg => true));
        }

        [Fact]
        public void OnlyIf_StandaloneBuilder()
        {
            var rb = RegistrationBuilder.ForType(typeof(object));
            Assert.Throws<NotSupportedException>(() => rb.OnlyIf(reg => true));
        }

        public class Decorator<T> : IService<T>
        {
            public Decorator(IService<T> decorated)
            {
                Decorated = decorated;
            }

            public IService<T> Decorated { get; }
        }

        public class Decorator : IService
        {
            public Decorator(IService decorated)
            {
                Decorated = decorated;
            }

            public IService Decorated { get; }
        }

        public class ServiceA : IService
        {
        }

        public class ServiceB : IService
        {
        }

        public class ServiceC : IService
        {
        }

        public interface IService2
        {
        }

        public class ServiceMulti : IService, IService2
        {
        }

        public class SimpleGeneric<T>
        {
        }

        public interface IService2<T>
        {
        }

        public class MultiServiceGeneric<T> : IService<T>, IService2<T>
        {
        }

        public class MultiServiceGeneric2<T> : IService<T>, IService2<T>
        {
        }

        public class MyModule : Module
        {
            private readonly Action<ContainerBuilder> _callback;

            public MyModule(Action<ContainerBuilder> callback)
            {
                _callback = callback;
            }

            protected override void Load(ContainerBuilder builder)
            {
                _callback(builder);
            }
        }
    }
}
