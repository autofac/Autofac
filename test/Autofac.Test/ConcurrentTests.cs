#if !WINDOWS_UWP
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Autofac.Test
{
    public class ConcurrentTests
    {
        [Fact]
        public void WhenTwoThreadsInitialiseASharedInstanceSimultaneouslyViaChildLifetime_OnlyOneInstanceIsActivated()
        {
            int activationCount = 0;
            var results = new ConcurrentBag<object>();
            var exceptions = new ConcurrentBag<Exception>();

            var builder = new ContainerBuilder();
            builder.Register(c =>
                {
                    Interlocked.Increment(ref activationCount);
                    Thread.Sleep(500);
                    return new object();
                })
                .SingleInstance();

            var container = builder.Build();

            ThreadStart work = () =>
            {
                 try
                 {
                     var o = container.BeginLifetimeScope().Resolve<object>();
                     results.Add(o);
                 }
                 catch (Exception ex)
                 {
                     exceptions.Add(ex);
                 }
            };

            var t1 = new Thread(work);
            var t2 = new Thread(work);
            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();

            Assert.Equal(1, activationCount);
            Assert.Empty(exceptions);
            Assert.Equal(1, results.Distinct().Count());
        }

        [Fact]
        public void ConcurrentResolveOperationsFromDifferentContainers_DoesNotThrow()
        {
            var task1 = Task.Factory.StartNew(ResolveObjectInstanceLoop);
            var task2 = Task.Factory.StartNew(ResolveObjectInstanceLoop);
            Task.WaitAll(task1, task2);
        }

        private static void ResolveObjectInstanceLoop()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<object>();
            var container = builder.Build();

            for (var index = 0; index < 100; index++)
            {
                container.Resolve<object>();
            }
        }
    }
}
#endif