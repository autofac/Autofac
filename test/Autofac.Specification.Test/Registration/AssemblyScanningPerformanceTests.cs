namespace Autofac.Specification.Test.Registration
{
    using System.Diagnostics;
    using Xunit;
    using Xunit.Abstractions;

    public class AssemblyScanningPerformanceTests
    {
        private readonly ITestOutputHelper output;

        public AssemblyScanningPerformanceTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void MeasurePerformance()
        {
            var builder = new ContainerBuilder();
            for (var i = 0; i < 1000; i++)
            {
                //just to simulate a lot of "scanning" with few (in this case zero) "hits"
                builder.RegisterAssemblyTypes(GetType().Assembly).Where(x => false);
            }

            var stopwatch = Stopwatch.StartNew();
            builder.Build().Dispose();

            //after fix drops from ~500ms to ~100ms
            output.WriteLine(stopwatch.Elapsed.TotalMilliseconds.ToString());
        }
    }
}