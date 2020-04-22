using Autofac.Test.Scenarios.GenericContraints;
using Xunit;

namespace Autofac.Test.Core.Resolving
{
    public class ResolveWithGenericTypeConstraintTests
    {
        [Fact]
        public void ResolveWithMultipleCandidatesLimitedByGenericConstraintsShouldSucceed()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterType<A>().As<IA>();
            containerBuilder.RegisterGeneric(typeof(Unrelated<>)).As(typeof(IB<>));
            containerBuilder.RegisterType<Required>().As<IB<ClassWithParameterlessButNotPublicConstructor>>();

            var container = containerBuilder.Build();
            var resolved = container.Resolve<IA>();
            Assert.NotNull(resolved);
        }
    }
}
