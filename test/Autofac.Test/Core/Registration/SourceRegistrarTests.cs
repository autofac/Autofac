// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Autofac.Core.Registration;
using Autofac.Test.Scenarios.RegistrationSources;
using Xunit;

namespace Autofac.Test.Core.Registration
{
    public sealed class SourceRegistrarTests
    {
        private static readonly object O1 = new object();
        private static readonly object O2 = new object();

        [Fact]
        public void Ctor_RequiresContainerBuilder()
        {
            Assert.Throws<ArgumentNullException>(() => new SourceRegistrar(null));
        }

        [Fact]
        public void RegisterSource_ChainsSourceRegistrations()
        {
            var builder = new ContainerBuilder();
            var registrar = new SourceRegistrar(builder);

            registrar.RegisterSource<SourceA>()
                .RegisterSource(new ObjectRegistrationSource(O2));

            var container = builder.Build();
            var objects = container.Resolve<IEnumerable<object>>();
            Assert.Contains(O1, objects);
            Assert.Contains(O2, objects);
        }

        [Fact]
        public void RegisterSource_RequiresRegistrationSource()
        {
            var registrar = new SourceRegistrar(new ContainerBuilder());
            Assert.Throws<ArgumentNullException>(() => registrar.RegisterSource(null));
        }

        private class SourceA : ObjectRegistrationSource
        {
            public SourceA()
                : base(O1)
            {
            }
        }
    }
}