// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Xunit;

namespace Autofac.Test.Core.Lifetime
{
    public class MatchingScopeLifetimeTests
    {
        [Fact]
        public void WhenNoMatchingScopeIsPresent_TheExceptionMessageIncludesTheTag()
        {
            var container = Factory.CreateEmptyContainer();
            const string tag = "abcdefg";
            var msl = new MatchingScopeLifetime(tag);
            var rootScope = (ISharingLifetimeScope)container.Resolve<ILifetimeScope>();

            var ex = Assert.Throws<DependencyResolutionException>(() => msl.FindScope(rootScope));
            Assert.Contains(tag, ex.Message);
        }

        [Fact]
        public void WhenNoMatchingScopeIsPresent_TheExceptionMessageIncludesTheTags()
        {
            var container = Factory.CreateEmptyContainer();
            const string tag1 = "abc";
            const string tag2 = "def";
            var msl = new MatchingScopeLifetime(tag1, tag2);
            var rootScope = (ISharingLifetimeScope)container.Resolve<ILifetimeScope>();

            var ex = Assert.Throws<DependencyResolutionException>(() => msl.FindScope(rootScope));
            Assert.Contains(string.Format("{0}, {1}", tag1, tag2), ex.Message);
        }

        [Fact]
        public void WhenTagsToMatchIsNull_ExceptionThrown()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => new MatchingScopeLifetime(null));

            Assert.Equal("lifetimeScopeTagsToMatch", exception.ParamName);
        }

        [Fact]
        public void MatchesAgainstSingleTaggedScope()
        {
            const string tag = "Tag";
            var msl = new MatchingScopeLifetime(tag);
            var container = Factory.CreateEmptyContainer();
            var lifetimeScope = (ISharingLifetimeScope)container.BeginLifetimeScope(tag);

            Assert.Equal(lifetimeScope, msl.FindScope(lifetimeScope));
        }

        [Fact]
        public void MatchesAgainstMultipleTaggedScopes()
        {
            const string tag1 = "Tag1";
            const string tag2 = "Tag2";

            var msl = new MatchingScopeLifetime(tag1, tag2);
            var container = Factory.CreateEmptyContainer();

            var tag1Scope = (ISharingLifetimeScope)container.BeginLifetimeScope(tag1);
            Assert.Equal(tag1Scope, msl.FindScope(tag1Scope));

            var tag2Scope = (ISharingLifetimeScope)container.BeginLifetimeScope(tag2);
            Assert.Equal(tag2Scope, msl.FindScope(tag2Scope));
        }
    }
}
