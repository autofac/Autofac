using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Autofac.Specification.Test.Registration
{
    public class LambdaRegistration
    {
        private interface IA
        {
        }

        [Fact]
        public void RegisterLambdaAsUnsupportedService()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => "hello").As<IA>();
            Assert.Throws<ArgumentException>(() => builder.Build());
        }
    }
}
