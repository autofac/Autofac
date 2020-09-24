// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Autofac.Features.Metadata;
using Xunit;

namespace Autofac.Specification.Test.Features
{
    public class MetadataTests
    {
        [Fact]
        public void WithMetadata()
        {
            var p1 = new KeyValuePair<string, object>("p1", "p1Value");
            var p2 = new KeyValuePair<string, object>("p2", "p2Value");

            var builder = new ContainerBuilder();
            builder.RegisterType<object>()
                .WithMetadata(p1.Key, p1.Value)
                .WithMetadata(p2.Key, p2.Value);

            var container = builder.Build();

            var obj = container.Resolve<Meta<object>>();
            Assert.Contains(p1, obj.Metadata);
            Assert.Contains(p2, obj.Metadata);
        }
    }
}
