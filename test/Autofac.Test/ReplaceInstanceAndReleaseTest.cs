using Autofac.Core;
using Xunit;

namespace Autofac.Test
{
    public class ReplaceInstanceAndReleaseTest
    {
        [Fact]
        public void ReplaceInstance_ReleaseTestWithOnActivating()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<someComponent>()
                .OnActivating(c => c.ReplaceInstance(new someComponent { IsReplaced = true }))
                .OnRelease(c => Assert.True(c.IsReplaced));

            using (var container = builder.Build())
            {
                container.Resolve<someComponent>();
            }
        }

        [Fact]
        public void ReplaceInstance_ReleaseTestWithModuleActivating()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<activatingModule>();
            builder.RegisterType<someComponent>()
                .OnRelease(c => Assert.True(c.IsReplaced));

            using (var container = builder.Build())
            {
                container.Resolve<someComponent>();
            }
        }

        private class activatingModule : Module
        {
            protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
            {
                registration.Activating += (o, args) =>
                {
                    args.ReplaceInstance(new someComponent { IsReplaced = true });
                };
            }
        }

        private class someComponent
        {
            public bool IsReplaced { get; set; }
        }
    }
}