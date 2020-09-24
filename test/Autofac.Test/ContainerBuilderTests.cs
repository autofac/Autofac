// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Autofac.Core.Resolving.Pipeline;
using Xunit;

namespace Autofac.Test
{
    public class ContainerBuilderTests
    {
        [Fact]
        public void RegisterBuildCallbackReturnsBuilderInstance()
        {
            var builder = new ContainerBuilder();
            Assert.Same(builder, builder.RegisterBuildCallback(c => { }));
        }

        [Fact]
        public void RegisterBuildCallbackThrowsWhenProvidedNullCallback()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(() => builder.RegisterBuildCallback(null));
            Assert.Equal("buildCallback", exception.ParamName);
        }

        [Fact]
        public void RegistrationsCanUsePropertyBag()
        {
            var builder = new ContainerBuilder();
            builder.Properties["count"] = 0;
            builder.Register(ctx =>
            {
                // TOTALLY not thread-safe, but illustrates the point.
                var count = (int)ctx.ComponentRegistry.Properties["count"];
                count++;
                ctx.ComponentRegistry.Properties["count"] = count;
                return "incremented";
            }).As<string>();
            var container = builder.Build();

            container.Resolve<string>();
            container.Resolve<string>();

            Assert.Equal(2, container.ComponentRegistry.Properties["count"]);
        }

        [Fact]
        public void WhenComponentIsRegisteredDuringResolveItShouldRaiseTheRegisteredEvent()
        {
            var activatedInstances = new List<object>();

            var builder = new ContainerBuilder();
            builder.RegisterCallback(x => x.Registered += (o, registration) =>
            {
                registration.ComponentRegistration.PipelineBuilding += (o, builder) =>
                    builder.Use(PipelinePhase.Activation, (ctxt, next) =>
                    {
                        next(ctxt);

                        activatedInstances.Add(ctxt.Instance);
                    });
            });

            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>));
            builder.RegisterType<Controller>().PropertiesAutowired();

            IContainer container = builder.Build();
            var controller = container.Resolve<Controller>();
            controller.UseTheRepository();

            Assert.Contains(activatedInstances, instance => instance is Controller);
            Assert.Contains(activatedInstances, instance => instance is IRepository<object>);
        }

        public interface IRepository<T>
        {
        }

        public class Repository<T> : IRepository<T>
        {
        }

        public class Controller
        {
            public Lazy<IRepository<object>> TheRepository { get; set; }

            public void UseTheRepository()
            {
                Assert.NotNull(TheRepository.Value);
            }
        }
    }
}
