using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac.Core.Activators.Reflection;

namespace Autofac.Test
{
    static class Mocks
    {
        public static IConstructorFinder GetConstructorFinder()
        {
            return new MockConstructorFinder();
        }

        public static IConstructorSelector GetConstructorSelector()
        {
            return new MockConstructorSelector();
        }

        public static IPropertyFinder GetPropertyFinder()
        {
            return new MockPropertyFinder();
        }

        public static Startable GetStartable()
        {
            return new Startable();
        }
    }

    class MockPropertyFinder : IPropertyFinder
    {
        public PropertyInfo[] FindProperties(Type properties)
        {
            return new PropertyInfo[0];
        }
    }

    class MockConstructorFinder : IConstructorFinder
    {
        public ConstructorInfo[] FindConstructors(Type targetType)
        {
            return new ConstructorInfo[0];
        }
    }

    class MockConstructorSelector : IConstructorSelector
    {
        public ConstructorParameterBinding SelectConstructorBinding(ConstructorParameterBinding[] constructorBindings)
        {
            return null;
        }
    }

    class Startable : IStartable
    {
        public int StartCount { get; private set; }

        public void Start()
        {
            StartCount++;
        }
    }
}