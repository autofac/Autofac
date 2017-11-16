using System;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Test.Scenarios.Dependencies;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Autofac.Test.Core.Resolving
{
    public class ResolveOperationAsyncTests
    {
        private static ManualResetEventSlim _gate = new ManualResetEventSlim(false);

        [Fact]
        public void CtorPropDependencyDetectDeadlock()
        {
            var cb = new ContainerBuilder().WithTimeout(TimeSpan.FromMilliseconds(100));
            cb.RegisterType<A>().SingleInstance();
            cb.RegisterType<B>().SingleInstance();

            var c = cb.Build();
            try
            {
                var dbp = c.Resolve<A>();
                throw new Exception("Unexpected");
            }
            catch (DependencyResolutionException ex)
            {
                var inner1 = ex.InnerException as AggregateException;
                if (!(inner1?.InnerException is TimeoutException))
                    throw new Exception("Unexpected");
            }
            finally
            {
                _gate.Set();
            }
        }

        public class A
        {
            public A(ILifetimeScope container)
            {
                Task.Run(() => container.Resolve<B>()).Wait(); // classic locking will cause deadlock
            }
        }

        public class B
        {
            public B()
            {
                if (!_gate.Wait(TimeSpan.FromSeconds(5)))
                    throw new Exception("Deadlock");
            }
        }
    }
}
