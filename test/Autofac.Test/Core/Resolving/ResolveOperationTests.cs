using System;
using System.Linq;
using Xunit;

namespace Autofac.Test.Core.Resolving
{
    public class ResolveOperationTests
    {
        [Fact]
        public void AfterTheOperationIsFinished_ReusingTheTemporaryContextThrows()
        {
            IComponentContext ctx = null;
            var builder = new ContainerBuilder();
            builder.Register(c =>
            {
                ctx = c;
                return new object();
            });
            builder.RegisterInstance("Hello");
            var container = builder.Build();
            container.Resolve<string>();
            container.Resolve<object>();
            Assert.Throws<ObjectDisposedException>(() => ctx.Resolve<string>());
        }
    }
}
