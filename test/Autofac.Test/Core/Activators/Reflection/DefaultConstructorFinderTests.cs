// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Core.Activators.Reflection;

namespace Autofac.Test.Core.Activators.Reflection;

public class DefaultConstructorFinderTests
{
    [Fact]
    public void FindsPublicConstructorsOnlyByDefault()
    {
        var finder = new DefaultConstructorFinder();
        var targetType = typeof(HasConstructors);
        var publicConstructor = targetType.GetConstructor(Array.Empty<Type>());

        var constructors = finder.FindConstructors(targetType).ToList();

        Assert.Single(constructors);
        Assert.Contains(publicConstructor, constructors);
    }

    [Fact]
    public void CanFindNonPublicConstructorsUsingFinderFunction()
    {
        var finder = new DefaultConstructorFinder(type => type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance));
        var targetType = typeof(HasConstructors);
        var privateConstructor = targetType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).Single();

        var constructors = finder.FindConstructors(targetType).ToList();

        Assert.Single(constructors);
        Assert.Contains(privateConstructor, constructors);
    }

    [Fact]
    public void SupportsZeroPublicConstructorTypes()
    {
        var finder = new DefaultConstructorFinder();
        var targetType = typeof(NoPublicConstructors);
        var constructors = finder.FindConstructors(targetType).ToList();

        Assert.Empty(constructors);
    }

    internal class HasConstructors
    {
        public HasConstructors()
        {
        }

        private HasConstructors(int value)
        {
        }
    }

    internal class NoPublicConstructors
    {
        private NoPublicConstructors()
        {
        }
    }
}
