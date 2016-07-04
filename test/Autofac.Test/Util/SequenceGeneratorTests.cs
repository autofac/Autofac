using Autofac.Util;
using Xunit;

namespace Autofac.Test.Util
{
    public class SequenceGeneratorTests
    {
        [Fact]
        public void AlwaysGeneratesUniqueAscendingSequenceNumbers()
        {
            var last = 0L;
            var next = SequenceGenerator.GetNextUniqueSequence();

            for (var i = 0; i < 100000; i++)
            {
                Assert.True(next > last);
                last = next;
                next = SequenceGenerator.GetNextUniqueSequence();
            }
        }
    }
}
