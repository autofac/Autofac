using System;
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

        public static Disposable GetDisposable()
        {
            return new Disposable();
        }

        public static Startable GetStartable()
        {
            return new Startable();
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

    class Disposable : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
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