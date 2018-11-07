using System;
using System.Threading.Tasks;
using Xunit;

namespace Autofac.Test.Concurrency
{
    public sealed class ConcurrencyTests
    {
        private sealed class A
        {
        }

        [Fact]
        public async Task RepeatedResolveWhileTheScopeIsDisposing_ObjectDisposedExceptionThrownOnly()
        {
            for (int i = 0; i < 100; i++)
            {
                await ResolveWhileTheScopeIsDisposing_ObjectDisposedExceptionThrownOnly();
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
    }
}
