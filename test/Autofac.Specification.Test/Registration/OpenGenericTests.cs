// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Test.Scenarios.Graph1.GenericContraints;
using Xunit;

namespace Autofac.Specification.Test.Registration
{
    public class OpenGenericTests
    {
        private interface IImplementedInterface<T>
        {
        }

        [Fact]
        public void AsImplementedInterfacesOnOpenGeneric()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(SelfComponent<>)).AsImplementedInterfaces();
            var context = builder.Build();
            context.Resolve<IImplementedInterface<object>>();
        }

        [Fact]
        public void AsSelfOnOpenGeneric()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(SelfComponent<>)).AsSelf();
            var context = builder.Build();
            context.Resolve<SelfComponent<object>>();
        }

        [Fact]
        public void ResolveWithMultipleCandidatesLimitedByGenericConstraintsShouldSucceed()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterType<A>().As<IA>();
            containerBuilder.RegisterGeneric(typeof(Unrelated<>)).As(typeof(IB<>));
            containerBuilder.RegisterType<Required>().As<IB<ClassWithParameterlessButNotPublicConstructor>>();

            var container = containerBuilder.Build();
            var resolved = container.Resolve<IA>();
            Assert.NotNull(resolved);
        }

        private class SelfComponent<T> : IImplementedInterface<T>
        {
        }
    }
}
