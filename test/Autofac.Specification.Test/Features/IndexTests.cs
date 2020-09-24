// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Features.Indexed;
using Xunit;

namespace Autofac.Specification.Test.Features
{
    public class IndexTests
    {
        [Fact]
        public void IndexCanRetrieveKeyedRegistration()
        {
            var key = 42;
            var cpt = "Hello";
            var builder = new ContainerBuilder();
            builder.RegisterInstance(cpt).Keyed<string>(key);
            var container = builder.Build();

            var idx = container.Resolve<IIndex<int, string>>();
            Assert.Same(cpt, idx[key]);
        }

        [Fact]
        public void IndexComposesWithIEnumerable()
        {
            var key = 42;
            var cpt = "Hello";
            var builder = new ContainerBuilder();
            builder.RegisterInstance(cpt).Keyed<string>(key);
            var container = builder.Build();

            var idx = container.Resolve<IIndex<int, IEnumerable<string>>>();
            Assert.Same(cpt, idx[key].Single());
        }
    }
}
