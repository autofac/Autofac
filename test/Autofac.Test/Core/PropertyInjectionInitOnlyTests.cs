// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Xunit;

#if NET5_0

namespace Autofac.Test.Core
{
    public class PropertyInjectionInitOnlyTests
    {
        public class HasInitOnlyProperties
        {
            public string InjectedString { get; init; }
        }

        [Fact]
        public void CanInjectInitOnlyProperties()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<HasInitOnlyProperties>().PropertiesAutowired();
            builder.Register(ctxt => "hello world");
            var container = builder.Build();

            var instance = container.Resolve<HasInitOnlyProperties>();

            Assert.Equal("hello world", instance.InjectedString);
        }
    }
}

#endif
