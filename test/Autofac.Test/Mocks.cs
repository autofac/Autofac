using System;
using System.Reflection;
using Autofac.Core.Activators.Reflection;

namespace Autofac.Test
{
    internal static class Mocks
    {
        public static IConstructorFinder GetConstructorFinder()
        {
            return new MockConstructorFinder();
        }

        public static IConstructorSelector GetConstructorSelector()
        {
            return new MockConstructorSelector();
        }

        public static Startable GetStartable()
        {
            return new Startable();
        }

        internal class MockConstructorFinder : IConstructorFinder
        {
            public ConstructorInfo[] FindConstructors(Type targetType)
            {
                return new ConstructorInfo[0];
            }
        }

        internal class MockConstructorSelector : IConstructorSelector
        {
            public ConstructorParameterBinding SelectConstructorBinding(ConstructorParameterBinding[] constructorBindings)
            {
                return null;
            }
        }

        internal class Startable : IStartable
        {
            public int StartCount { get; private set; }

            public void Start()
            {
                StartCount++;
            }
        }
    }
}