// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Xunit;

namespace Autofac.Specification.Test.Lifetime
{
    /// <summary>
    /// Tests involving the lifetime of a nested scope. Tests for registering objects in a nested scope are
    /// in <see cref="Autofac.Specification.Test.Registration.NestedScopeRegistrationTests"/>. Tests for specifics
    /// around disposal are in <see cref="Autofac.Specification.Test.Lifetime.DisposalTests"/> or in the fixture
    /// for the specific lifetime model being tested (singleton, provided instance, etc.).
    /// </summary>
    public class NestedScopeTests
    {
        [Fact]
        public void BeginLifetimeScopeCannotBeCalledWithDuplicateTag()
        {
            var rootScope = new ContainerBuilder().Build();
            const string duplicateTagName = "ABC";
            var taggedScope = rootScope.BeginLifetimeScope(duplicateTagName);
            var differentTaggedScope = taggedScope.BeginLifetimeScope("DEF");

            Assert.Throws<InvalidOperationException>(() => differentTaggedScope.BeginLifetimeScope(duplicateTagName));
            Assert.Throws<InvalidOperationException>(() => differentTaggedScope.BeginLifetimeScope(duplicateTagName, builder => builder.RegisterType<object>()));
        }
    }
}
