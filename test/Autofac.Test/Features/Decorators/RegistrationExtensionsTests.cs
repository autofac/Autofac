// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Autofac.Core;
using Xunit;

namespace Autofac.Test.Features.Decorators
{
    public class RegistrationExtensionsTests
    {
        [Fact]
        public void RegisterDecoratorRequiresDecoratorType()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.RegisterDecorator(null, typeof(object)));

            Assert.Equal("decoratorType", exception.ParamName);
        }

        [Fact]
        public void RegisterDecoratorRequiresServiceType()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.RegisterDecorator(typeof(object), null));

            Assert.Equal("serviceType", exception.ParamName);
        }

        [Fact]
        public void RegisterDecoratorRequiresDecoratorFunc()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.RegisterDecorator(
                    default(Func<IComponentContext, IEnumerable<Parameter>, object, object>)));

            Assert.Equal("decorator", exception.ParamName);
        }

        [Fact]
        public void RegisterGenericDecoratorRequiresDecoratorType()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.RegisterGenericDecorator(null, typeof(object)));

            Assert.Equal("decoratorType", exception.ParamName);
        }

        [Fact]
        public void RegisterGenericDecoratorRequiresServiceType()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.RegisterGenericDecorator(typeof(object), null));

            Assert.Equal("serviceType", exception.ParamName);
        }
    }
}
