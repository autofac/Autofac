// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.ResolveAnything;
using Xunit;

namespace Autofac.Test.Concurrency
{
    public sealed class ConcurrencyTests
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
            var unblocked = 0;
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

            Interlocked.MemoryBarrier();
            Assert.Equal(0, unblocked);
            evt.Set();
            blockedThread.Join();
        }

        [Fact]
        public void ConcurrentResolveOperationsFromDifferentContainers_DoesNotThrow()
        {
            var task1 = Task.Factory.StartNew(ResolveObjectInstanceLoop);
            var task2 = Task.Factory.StartNew(ResolveObjectInstanceLoop);
            Task.WaitAll(task1, task2);
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

        [Fact]
        public async Task RepeatedResolveWhileTheScopeIsDisposing_ObjectDisposedExceptionThrownOnly()
        {
            for (var i = 0; i < 100; i++)
            {
                await ResolveWhileTheScopeIsDisposing_ObjectDisposedExceptionThrownOnly();
            }
        }

        [Fact]
        public void WhenTwoThreadsInitialiseASharedInstanceSimultaneouslyViaChildLifetime_OnlyOneInstanceIsActivated()
        {
            var activationCount = 0;
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
            Assert.Single(results.Distinct());
        }

        [Fact]
        public void WhenSeveralThreadsResolveNotAlreadyRegisteredType_DoesNotThrow()
        {
            for (var i = 0; i < 20_000; i++)
            {
                var builder = new ContainerBuilder();
                builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
                var container = builder.Build();
                Parallel.Invoke(() => container.Resolve<A>(), () => container.Resolve<A>());
            }
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

        private async Task ResolveWhileTheScopeIsDisposing_ObjectDisposedExceptionThrownOnly()
        {
            var cb = new ContainerBuilder();
            var container = cb.Build();

            var scope = container.BeginLifetimeScope(builder => builder.RegisterType<A>());

            var t = Task.Run(
                () =>
                {
                    try
                    {
                        while (true)
                        {
                            scope.Resolve<A>();
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                });
            await Task.Delay(5);

            scope.Dispose();

            await t;
        }

        private sealed class A
        {
        }
    }
}
