using System;
using System.Linq;
using System.Threading;
using Xunit;

namespace Autofac.Net46.Test
{
    public class ConcurrentTests
    {
        [Fact]
        public void ConcurrentResolveOperationsForNonSharedInstancesFromDifferentLifetimes_DoNotBlock()
        {
            var evt = new ManualResetEvent(false);

            var builder = new ContainerBuilder();
            builder.Register((c, p) =>
                {
                    if (p.TypedAs<bool>())
                    {
                        evt.WaitOne();
                    }

                    return new object();
                });

            var container = builder.Build();
            int unblocked = 0;
            var blockedScope = container.BeginLifetimeScope();
            var blockedThread = new Thread(() =>
            {
                blockedScope.Resolve<object>(TypedParameter.From(true));
                Interlocked.Increment(ref unblocked);
            });
            blockedThread.Start();
            Thread.Sleep(500);

            container.Resolve<object>(TypedParameter.From(false));
            container.BeginLifetimeScope().Resolve<object>(TypedParameter.From(false));

            Thread.MemoryBarrier();
            Assert.Equal(0, unblocked);
            evt.Set();
            blockedThread.Join();
        }

        [Fact]
        public void NoLockWhenResolvingExistingSingleInstance()
        {
            var builder = new ContainerBuilder();
            var containerProvider = default(Func<IContainer>);
            builder.Register(c => default(int)).SingleInstance();
            builder.Register(c =>
            {
                using (var mres = new ManualResetEventSlim())
                {
                    ThreadPool.QueueUserWorkItem(state =>
                    {
                        containerProvider().Resolve<int>();
                        mres.Set();
                    });
                    mres.Wait(1250);
                }
                return new object();
            });

            var container = builder.Build();
            containerProvider = () => container;
            container.Resolve<int>();
            container.Resolve<object>();
        }
    }
}
