using System;
using Xunit;

namespace Autofac.Specification.Test.Registration
{
    public class ParameterTests
    {
        [Fact]
        public void NamedParametersProvided()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<WithParam>()
                .WithParameter(new NamedParameter("i", 5))
                .WithParameter(new NamedParameter("j", 10));

            var c = cb.Build();
            var result = c.Resolve<WithParam>();

            Assert.NotNull(result);
            Assert.Equal(15, result.Value);
        }

        private class WithParam
        {
            public WithParam(int i, int j)
            {
                this.Value = i + j;
            }

            public int Value { get; private set; }
        }
    }
}
