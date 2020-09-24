// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using Autofac.Core;
using Autofac.Test.Scenarios.Dependencies.Circularity;
using Xunit;

namespace Autofac.Test
{
    public class CircularDependencyTests
    {
        [Fact]
        public void IdentifiesCircularDependencyInExceptionMessage()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<D>().As<ID>();
            builder.RegisterType<A>().As<IA>();
            builder.RegisterType<BC>().As<IB, IC>();
            var container = builder.Build();
            var de = Assert.Throws<DependencyResolutionException>(() => container.Resolve<ID>());
            Assert.Contains("Autofac.Test.Scenarios.Dependencies.Circularity.D -> Autofac.Test.Scenarios.Dependencies.Circularity.A -> Autofac.Test.Scenarios.Dependencies.Circularity.BC -> Autofac.Test.Scenarios.Dependencies.Circularity.A", de.ToString());
        }
    }
}
