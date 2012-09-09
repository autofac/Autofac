using System;
using Autofac;
using Autofac.Extras.Multitenant;
using Autofac.Extras.Tests.Multitenant.Stubs;
using NUnit.Framework;

namespace Autofac.Extras.Tests.Multitenant
{
    [TestFixture]
    public class ConfigurationActionBuilderFixture
    {
        [Test(Description = "Even if no actions are registered, an action should be returned on Build.")]
        public void Build_NoActionsRegistered()
        {
            var builder = new ConfigurationActionBuilder();
            Assert.IsNotNull(builder.Build(), "Even if nothing is registered, the built action should not be null.");
        }

        [Test(Description = "If multiple actions are registered, they should all be aggregated.")]
        public void Build_MultipleActionsRegistered()
        {
            var builder = new ConfigurationActionBuilder();
            builder.Add(b => b.RegisterType<StubDependency1Impl1>().As<IStubDependency1>());
            builder.Add(b => b.RegisterType<StubDependency2Impl1>().As<IStubDependency2>());
            var built = builder.Build();

            var container = new ContainerBuilder().Build();
            using (var scope = container.BeginLifetimeScope(built))
            {
                Assert.IsInstanceOf<StubDependency1Impl1>(scope.Resolve<IStubDependency1>(), "The first registered lambda did not execute.");
                Assert.IsInstanceOf<StubDependency2Impl1>(scope.Resolve<IStubDependency2>(), "The second registered lambda did not execute.");
            }
        }

        [Test(Description = "If only one action is registered, it should be aggregated.")]
        public void Build_SingleActionRegistered()
        {
            var builder = new ConfigurationActionBuilder();
            builder.Add(b => b.RegisterType<StubDependency1Impl1>().As<IStubDependency1>());
            var built = builder.Build();

            var container = new ContainerBuilder().Build();
            using (var scope = container.BeginLifetimeScope(built))
            {
                Assert.IsInstanceOf<StubDependency1Impl1>(scope.Resolve<IStubDependency1>(), "The registered lambda did not execute.");
            }
        }
    }
}
