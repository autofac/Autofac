using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Autofac.Core;
using Autofac.Core.Resolving;
using Xunit;

namespace Autofac.Test.Core.Resolving
{
    public class CircularDependencyDetectorTests
    {
        [Fact]
        public void OnCircularDependency_MessageDescribesCycle()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => c.Resolve<object>());

            var target = builder.Build();
            var de = Assert.Throws<DependencyResolutionException>(() => target.Resolve<object>());
            Assert.Contains("System.Object -> System.Object", de.Message);
            Assert.DoesNotContain("System.Object -> System.Object -> System.Object", de.Message);
        }

        [Fact]
        public void MaxStackDepthExceeded_ThrowsCircularDependencyException()
        {
            var builder = new ContainerBuilder().WithMaxResolveStackDepth(10);
            builder.RegisterType<OuterScopeType>();
            builder.RegisterType<OuterScopeType2>();
            builder.RegisterType<OuterScopeType3>();
            builder.RegisterType<OuterScopeType4>();
            builder.RegisterType<OuterScopeType5>();
            IContainer container = builder.Build();

            using (var lifetimeScope = container.BeginLifetimeScope(b => b.RegisterType<InnerScopeType>()))
            {
                Assert.Throws<DependencyResolutionException>(() => lifetimeScope.Resolve<InnerScopeType>());
            }
        }

        [Fact]
        public void MaxStackDepthNotExceeded_CanBeResolved()
        {
            var builder = new ContainerBuilder().WithMaxResolveStackDepth(11);
            builder.RegisterType<OuterScopeType>();
            builder.RegisterType<OuterScopeType2>();
            builder.RegisterType<OuterScopeType3>();
            builder.RegisterType<OuterScopeType4>();
            builder.RegisterType<OuterScopeType5>();
            IContainer container = builder.Build();

            using (var lifetimeScope = container.BeginLifetimeScope(b => b.RegisterType<InnerScopeType>()))
            {
                Assert.NotNull(lifetimeScope.Resolve<InnerScopeType>());
            }
        }

        [Fact(Skip = "Issue #648")]
        public void ManualEnumerableRegistrationDoesNotCauseCircularDependency()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<RootViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<PluginsViewModel>().AsSelf().SingleInstance();

            builder.RegisterType(typeof(Plugin1)).Named<IPlugin>("Plugin1");
            builder.RegisterType(typeof(Plugin2)).Named<IPlugin>("Plugin2");
            builder.Register(
                ctx => new[] { "Plugin1", "Plugin2" }
                    .Select(name => SafeResolvePlugin(name, ctx))
                    .Where(p => p != null)
                    .ToArray())
                .As<IEnumerable<IPlugin>>()
                .SingleInstance();

            var container = builder.Build();

            // From issue 648, this resolve call was getting a circular dependency
            // detection exception. It shouldn't be getting anything because the "safe resolve"
            // eats the dependency resolution issue for Plugin2 and Plugin1 should be
            // properly resolved.
            Assert.NotNull(container.Resolve<RootViewModel>());
            Assert.Single(container.Resolve<IEnumerable<IPlugin>>());
        }

        private static IPlugin SafeResolvePlugin(string pluginName, IComponentContext core)
        {
            try
            {
                // Plugin2 will get filtered out because it has an
                // unavailable dependency.
                return core.ResolveNamed<IPlugin>(pluginName);
            }
            catch (DependencyResolutionException)
            {
                return null;
            }
        }

        private interface IPlugin
        {
        }

        private class Plugin1 : IPlugin
        {
        }

        private class Plugin2 : IPlugin
        {
            public Plugin2(IUnavailableComponent unavailableComponent)
            {
            }
        }

        private interface IUnavailableComponent
        {
        }

        private class PluginsViewModel
        {
            public PluginsViewModel(IEnumerable<IPlugin> plugins)
            {
            }
        }

        private class RootViewModel
        {
            public RootViewModel(IEnumerable<IPlugin> plugins, PluginsViewModel pluginsViewModel)
            {
            }
        }

        public class OuterScopeType
        {
            public OuterScopeType(OuterScopeType2 outerScopeType2) { }
        }

        public class OuterScopeType2
        {
            public OuterScopeType2(OuterScopeType3 outerScopeType3) { }
        }

        public class OuterScopeType3
        {
            public OuterScopeType3(OuterScopeType4 outerScopeType4) { }

        }

        public class OuterScopeType4
        {
            public OuterScopeType4(OuterScopeType5 outerScopeType5) { }
        }

        public class OuterScopeType5 { }

        public class InnerScopeType
        {
            public InnerScopeType(
                OuterScopeType outerScopeType,
                OuterScopeType4 outerScopeType4,
                OuterScopeType5 outerScopeType5
            )
            {
            }
        }
    }
}
