using System;
using Autofac.Core;
using Xunit;

namespace Autofac.Test.Core
{
    // ReSharper disable ClassNeverInstantiated.Local, UnusedParameter.Local
    public class DependencyResolutionExceptionTests
    {
        public class A
        {
            public const string Message = "This is the original exception.";

            public A()
            {
                throw new InvalidOperationException(Message);
            }
        }

        public class B
        {
            public B(A a)
            {
            }
        }

        public class C
        {
            public C(B b)
            {
            }
        }

        [Fact]
        public void ExceptionMessageUnwrapsNestedResolutionFailures()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<A>();
            builder.Register(c => new B(c.Resolve<A>()));
            builder.RegisterType<C>();

            Exception ex;
            using (var container = builder.Build())
            {
                ex = Assert.Throws<DependencyResolutionException>(() => container.Resolve<C>());
            }

            var n = GetType().FullName;
            Assert.Equal($"An exception was thrown while activating {n}+C -> λ:{n}+B -> {n}+A.", ex.Message);

            var inner = ex.InnerException;
            Assert.IsType<DependencyResolutionException>(inner);
            Assert.Equal("An exception was thrown while invoking the constructor 'Void .ctor()' on type 'A'.", inner.Message);

            Assert.IsType<InvalidOperationException>(inner.InnerException);
            Assert.Equal(A.Message, inner.InnerException.Message);
        }
    }
}
