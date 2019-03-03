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

            // Without unwrapping, the exception message is:
            //
            // An error occurred during the activation of a particular registration. See the inner exception
            // for details. Registration: Activator = C (ReflectionActivator), Services =
            // [Autofac.Test.ExceptionReportingTests+C], Lifetime = Autofac.Core.Lifetime.CurrentScopeLifetime,
            // Sharing = None, Ownership = OwnedByLifetimeScope ---> An error occurred during the activation of
            // a particular registration. See the inner exception for details. Registration: Activator = B
            // (DelegateActivator), Services = [Autofac.Test.ExceptionReportingTests+B], Lifetime =
            // Autofac.Core.Lifetime.CurrentScopeLifetime, Sharing = None, Ownership = OwnedByLifetimeScope --->
            // An error occurred during the activation of a particular registration. See the inner exception for
            // details. Registration: Activator = A (ReflectionActivator), Services =
            // [Autofac.Test.ExceptionReportingTests+A], Lifetime = Autofac.Core.Lifetime.CurrentScopeLifetime,
            // Sharing = None, Ownership = OwnedByLifetimeScope ---> An exception was thrown while invoking the
            // constructor 'Void .ctor()' on type 'A'. ---> This is the original exception. (See inner exception
            // for details.) (See inner exception for details.) (See inner exception for details.) (See inner
            // exception for details.)
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
