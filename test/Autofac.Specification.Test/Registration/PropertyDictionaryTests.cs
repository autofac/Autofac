// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Xunit;

namespace Autofac.Specification.Test.Registration
{
    public class PropertyDictionaryTests
    {
        [Fact]
        public void ChildScopeBuilderGetsParentProperties()
        {
            var builder = new ContainerBuilder();
            builder.Properties["count"] = 5;
            var container = builder.Build();

            using (var outerScope = container.BeginLifetimeScope(b =>
            {
                Assert.Equal(5, b.Properties["count"]);
                b.Properties["count"] = 10;
            }))
            {
                Assert.Equal(10, outerScope.ComponentRegistry.Properties["count"]);
                using (var innerScope = outerScope.BeginLifetimeScope(b =>
                {
                    Assert.Equal(10, b.Properties["count"]);
                    b.Properties["count"] = 15;
                }))
                {
                    Assert.Equal(5, container.ComponentRegistry.Properties["count"]);
                    Assert.Equal(10, outerScope.ComponentRegistry.Properties["count"]);
                    Assert.Equal(15, innerScope.ComponentRegistry.Properties["count"]);
                }
            }
        }

        [Fact]
        public void LambdaRegistrationsDoNotAffectPropertyPropagation()
        {
            // In the past we've had issues where lambda configuration
            // expressions change the behavior of builder/container semantics.
            // This ensures we can use properties even when we aren't using
            // lambdas in lifetime scope startup.
            var builder = new ContainerBuilder();
            builder.Properties["count"] = 5;
            var container = builder.Build();

            using (var outerScope = container.BeginLifetimeScope())
            {
                using (var innerScope = outerScope.BeginLifetimeScope(b =>
                {
                    b.Properties["count"] = 15;
                }))
                {
                    Assert.Equal(5, container.ComponentRegistry.Properties["count"]);
                    Assert.Equal(5, outerScope.ComponentRegistry.Properties["count"]);
                    Assert.Equal(15, innerScope.ComponentRegistry.Properties["count"]);
                }
            }
        }

        [Fact]
        public void RegistrationsCanUseContextProperties()
        {
            var builder = new ContainerBuilder();
            builder.Properties["count"] = 0;
            builder.Register(ctx =>
            {
                return ctx.ComponentRegistry.Properties["count"].ToString();
            }).As<string>();

            var container = builder.Build();

            Assert.Equal("0", container.Resolve<string>());
            using (var scope = container.BeginLifetimeScope(b => b.Properties["count"] = 1))
            {
                Assert.Equal("1", scope.Resolve<string>());
            }

            Assert.Equal("0", container.Resolve<string>());
        }
    }
}
