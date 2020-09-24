// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Core;
using Autofac.Features.Metadata;
using Autofac.Features.OwnedInstances;
using Xunit;

namespace Autofac.Test.Features.Collections
{
    public class CollectionOrderingTests
    {
        private const string S1 = "s1";
        private const string S2 = "s2";
        private const string S3 = "s3";

        public interface IService
        {
        }

        public class Implementer1 : IService
        {
        }

        public class Implementer2 : IService
        {
        }

        public class Implementer3 : IService
        {
        }

        public class Decorator : IService
        {
            public Decorator(IService decorated)
            {
                Decorated = decorated;
            }

            public IService Decorated { get; }
        }

        private class Command
        {
            public string CommandId { get; }

            public Command(string commandId)
            {
                CommandId = commandId;
            }
        }

        private interface ICommandAdaptor
        {
            Command Command { get; }
        }

        private class CommandAdaptor : ICommandAdaptor
        {
            public Command Command { get; }

            public CommandAdaptor(Command command)
            {
                Command = command;
            }
        }

        private class StringRegistrationSource : IRegistrationSource
        {
            private readonly string[] _instances;
            private readonly Service _stringService = new TypedService(typeof(string));

            public StringRegistrationSource(params string[] instances)
            {
                _instances = instances;
            }

            public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
            {
                return service == _stringService
                    ? _instances.Select(Factory.CreateSingletonRegistration)
                    : Enumerable.Empty<IComponentRegistration>();
            }

            public bool IsAdapterForIndividualComponents => false;
        }

        private class ImplementorModule : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                builder.Register(c => new Implementer1()).As<IService>();
                builder.Register(c => new Implementer2()).As<IService>();
            }
        }

        [Fact]
        public void WhenRegisteredWithRegisterType()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<Implementer1>().As<IService>();
            cb.RegisterType<Implementer2>().As<IService>();
            cb.RegisterType<Implementer3>().As<IService>();
            var c = cb.Build();

            var services = c.Resolve<IEnumerable<IService>>().ToArray();

            Assert.IsType<Implementer1>(services[0]);
            Assert.IsType<Implementer2>(services[1]);
            Assert.IsType<Implementer3>(services[2]);
        }

        [Fact]
        public void WhenRegisteredWithRegisterDelegate()
        {
            var cb = new ContainerBuilder();
            cb.Register(c => S1);
            cb.Register(c => S2);
            cb.Register(c => S3);
            var container = cb.Build();

            var services = container.Resolve<IEnumerable<string>>().ToArray();

            Assert.Equal(S1, services[0]);
            Assert.Equal(S2, services[1]);
            Assert.Equal(S3, services[2]);
        }

        [Fact]
        public void WhenRegisteredWithRegisterInstance()
        {
            var cb = new ContainerBuilder();
            cb.RegisterInstance(S1);
            cb.RegisterInstance(S2);
            cb.RegisterInstance(S3);
            var c = cb.Build();

            var services = c.Resolve<IEnumerable<string>>().ToArray();

            Assert.Equal(S1, services[0]);
            Assert.Equal(S2, services[1]);
            Assert.Equal(S3, services[2]);
        }

        [Fact]
        public void WhenRegisteredWithMixedRegistrationMethods()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<Implementer1>().As<IService>();
            cb.Register(c => new Implementer2()).As<IService>();
            cb.RegisterInstance(new Implementer3()).As<IService>();
            var container = cb.Build();

            var services = container.Resolve<IEnumerable<IService>>().ToArray();

            Assert.IsType<Implementer1>(services[0]);
            Assert.IsType<Implementer2>(services[1]);
            Assert.IsType<Implementer3>(services[2]);
        }

        [Fact]
        public void WhenRegisteredWithPreviousPreserveExistingDefaults()
        {
            var cb = new ContainerBuilder();
            cb.RegisterInstance(S1);
            cb.RegisterInstance(S2).PreserveExistingDefaults();
            cb.RegisterInstance(S3);
            var c = cb.Build();

            var services = c.Resolve<IEnumerable<string>>().ToArray();

            Assert.Equal(S1, services[0]);
            Assert.Equal(S2, services[1]);
            Assert.Equal(S3, services[2]);
        }

        [Fact]
        public void WhenRegisteredInNestedLifetimeScope()
        {
            var cb = new ContainerBuilder();
            cb.RegisterInstance(S1);
            cb.RegisterInstance(S2);
            var c = cb.Build();
            var ls = c.BeginLifetimeScope(b => b.RegisterInstance(S3));

            var services = ls.Resolve<IEnumerable<string>>().ToArray();

            Assert.Equal(S1, services[0]);
            Assert.Equal(S2, services[1]);
            Assert.Equal(S3, services[2]);
        }

        [Fact]
        public void WhenRegisteredInNestedLifetimeScopeAndPreserveExistingDefaults()
        {
            var cb = new ContainerBuilder();
            cb.RegisterInstance(S1);
            cb.RegisterInstance(S2).PreserveExistingDefaults();
            var c = cb.Build();
            var ls = c.BeginLifetimeScope(b => b.RegisterInstance(S3));

            var services = ls.Resolve<IEnumerable<string>>().ToArray();

            Assert.Equal(S1, services[0]);
            Assert.Equal(S2, services[1]);
            Assert.Equal(S3, services[2]);
        }

        [Fact]
        public void WhenRegisteredInMultipleNestedLifetimeScopes()
        {
            var cb = new ContainerBuilder();
            cb.RegisterInstance(S1);
            var c = cb.Build();
            var ls = c.BeginLifetimeScope(b => b.RegisterInstance(S2));
            var ls2 = ls.BeginLifetimeScope(b => b.RegisterInstance(S3));

            var services = ls2.Resolve<IEnumerable<string>>().ToArray();

            Assert.Equal(S1, services[0]);
            Assert.Equal(S2, services[1]);
            Assert.Equal(S3, services[2]);
        }

        [Fact]
        public void WhenRegisteredInNestedLifetimeScopeAndUserDefinedRegistrationSourceInParentLifetimeScope()
        {
            var cb = new ContainerBuilder();
            cb.RegisterInstance(S1);
            cb.RegisterSource(new StringRegistrationSource(S2));
            var c = cb.Build();
            var ls = c.BeginLifetimeScope(b => b.RegisterInstance(S3));

            var services = ls.Resolve<IEnumerable<string>>().ToArray();

            Assert.Equal(S1, services[0]);
            Assert.Equal(S3, services[1]);
            Assert.Equal(S2, services[2]); // External registration sources always come last.
        }

        [Fact]
        public void WhenRegisteredInNestedLifetimeScopeAndUserDefinedRegistrationSourceInChildLifetimeScope()
        {
            var cb = new ContainerBuilder();
            cb.RegisterInstance(S1);
            var c = cb.Build();
            var ls = c.BeginLifetimeScope(b =>
            {
                b.RegisterSource(new StringRegistrationSource(S2));
                b.RegisterInstance(S3);
            });

            var services = ls.Resolve<IEnumerable<string>>().ToArray();

            Assert.Equal(S1, services[0]);
            Assert.Equal(S3, services[1]);
            Assert.Equal(S2, services[2]); // External registration sources always come last.
        }

        [Fact]
        public void WhenRegisteredWithAssemblyScanningExplicitRegistrationsComeFirst()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(IService).GetTypeInfo().Assembly)
                .Where(t => t.IsAssignableTo<IService>() && !t.IsAssignableTo<Implementer3>())
                .As<IService>();
            cb.RegisterType<Implementer3>().As<IService>();
            var c = cb.Build();

            var services = c.Resolve<IEnumerable<IService>>().ToArray();

            Assert.IsType<Implementer3>(services[0]); // Explicit registrations come first.
            Assert.Contains(services, service => service is Implementer1); // Order indeterminate as based on reflection.
            Assert.Contains(services, service => service is Implementer2); // Order indeterminate as based on reflection.
        }

        [Fact]
        public void WhenRegisteredWithManuallyActivatedModule()
        {
            var cb = new ContainerBuilder();
            cb.RegisterModule(new ImplementorModule());
            cb.RegisterType<Implementer3>().As<IService>();
            var container = cb.Build();

            var services = container.Resolve<IEnumerable<IService>>().ToArray();

            Assert.IsType<Implementer3>(services[0]); // Explicit registrations come first.
            Assert.IsType<Implementer1>(services[1]); // Module registration are in added order.
            Assert.IsType<Implementer2>(services[2]); // Module registration are in added order.
        }

        [Fact]
        public void WhenRegisteredWithContainerActivatedModule()
        {
            var cb = new ContainerBuilder();
            cb.RegisterModule<ImplementorModule>();
            cb.RegisterType<Implementer3>().As<IService>();
            var container = cb.Build();

            var services = container.Resolve<IEnumerable<IService>>().ToArray();

            Assert.IsType<Implementer3>(services[0]); // Explicit registrations come first.
            Assert.IsType<Implementer1>(services[1]); // Module registration are in added order.
            Assert.IsType<Implementer2>(services[2]); // Module registration are in added order.
        }

        [Fact]
        public void WhenRegisteredWithUserDefinedRegistrationSource()
        {
            var cb = new ContainerBuilder();
            cb.RegisterSource(new StringRegistrationSource(S1, S2));
            cb.RegisterInstance(S3);
            var c = cb.Build();

            var services = c.Resolve<IEnumerable<string>>().ToArray();

            Assert.Equal(S3, services[0]); // Explicit registrations come first.
            Assert.Equal(S1, services[1]); // Registration sources are in creation order.
            Assert.Equal(S2, services[2]); // Registration sources are in creation order.
        }

        [Fact]
        public void WhenResolvedWithFactory()
        {
            var cb = new ContainerBuilder();
            cb.RegisterInstance(S1);
            cb.RegisterInstance(S2);
            cb.RegisterInstance(S3);
            var c = cb.Build();

            var services = c.Resolve<IEnumerable<Func<string>>>().ToArray();

            Assert.Equal(S1, services[0]());
            Assert.Equal(S2, services[1]());
            Assert.Equal(S3, services[2]());
        }

        [Fact]
        public void WhenResolvedWithMetadata()
        {
            var cb = new ContainerBuilder();
            cb.RegisterInstance(S1).WithMetadata("foo", "m1");
            cb.RegisterInstance(S2).WithMetadata("foo", "m2");
            cb.RegisterInstance(S3).WithMetadata("foo", "m3");
            var c = cb.Build();

            var services = c.Resolve<IEnumerable<Meta<string>>>().ToArray();

            Assert.Equal(S1, services[0].Value);
            Assert.Equal(S2, services[1].Value);
            Assert.Equal(S3, services[2].Value);
        }

        [Fact]
        public void WhenResolvedWithLazy()
        {
            var cb = new ContainerBuilder();
            cb.RegisterInstance(S1);
            cb.RegisterInstance(S2);
            cb.RegisterInstance(S3);
            var c = cb.Build();

            var services = c.Resolve<IEnumerable<Lazy<string>>>().ToArray();

            Assert.Equal(S1, services[0].Value);
            Assert.Equal(S2, services[1].Value);
            Assert.Equal(S3, services[2].Value);
        }

        [Fact]
        public void WhenResolvedWithOwned()
        {
            var cb = new ContainerBuilder();
            cb.RegisterInstance(S1);
            cb.RegisterInstance(S2);
            cb.RegisterInstance(S3);
            var c = cb.Build();

            var services = c.Resolve<IEnumerable<Owned<string>>>().ToArray();

            Assert.Equal(S1, services[0].Value);
            Assert.Equal(S2, services[1].Value);
            Assert.Equal(S3, services[2].Value);
        }

        [Fact]
        public void WhenResolvedThroughAdaptor()
        {
            var cb = new ContainerBuilder();
            cb.RegisterInstance(new Command(S1));
            cb.RegisterInstance(new Command(S2));
            cb.RegisterInstance(new Command(S3));
            cb.RegisterAdapter<Command, CommandAdaptor>(s => new CommandAdaptor(s)).As<ICommandAdaptor>();
            var container = cb.Build();

            var services = container.Resolve<IEnumerable<ICommandAdaptor>>().ToArray();

            Assert.Equal(S1, services[0].Command.CommandId);
            Assert.Equal(S2, services[1].Command.CommandId);
            Assert.Equal(S3, services[2].Command.CommandId);
        }

        [Fact]
        public void WhenResolvedWithDecorator()
        {
            const string from = "from";
            var cb = new ContainerBuilder();
            cb.RegisterType<Implementer1>().Named<IService>(from);
            cb.RegisterType<Implementer2>().Named<IService>(from);
            cb.RegisterType<Implementer3>().Named<IService>(from);
            cb.RegisterDecorator<IService>(s => new Decorator(s), from);
            var container = cb.Build();

            var services = container.Resolve<IEnumerable<IService>>().Cast<Decorator>().ToArray();

            Assert.IsType<Implementer1>(services[0].Decorated);
            Assert.IsType<Implementer2>(services[1].Decorated);
            Assert.IsType<Implementer3>(services[2].Decorated);
        }

        [Fact]
        public void WhenResolvedWithMultipleAdaptors()
        {
            var cb = new ContainerBuilder();
            cb.RegisterInstance(S1);
            cb.RegisterInstance(S2);
            cb.RegisterInstance(S3);
            var c = cb.Build();

            var services = c.Resolve<IEnumerable<Lazy<Func<Owned<Meta<string>>>>>>().ToArray();

            Assert.Equal(S1, services[0].Value().Value.Value);
            Assert.Equal(S2, services[1].Value().Value.Value);
            Assert.Equal(S3, services[2].Value().Value.Value);
        }
    }
}
