using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Features.AttributeFilters;
using Autofac.Features.Metadata;
using Autofac.Features.OwnedInstances;
using Xunit;

namespace Autofac.Test.Features.AttributeFilters
{
    public class WithAttributeFilterTestFixture
    {
        [Fact]
        public void MultipleFilterTypesCanBeUsedOnOneComponent()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<MsBuildAdapter>()
                .WithMetadata<AdapterMetadata>(m => m.For(am => am.Target, "Solution"))
                .As<IAdapter>();

            builder.RegisterType<DteAdapter>()
                .WithMetadata<AdapterMetadata>(m => m.For(am => am.Target, "Solution"))
                .As<IAdapter>();

            builder.RegisterType<ToolWindowAdapter>()
                .WithMetadata<AdapterMetadata>(m => m.For(am => am.Target, "Other"))
                .As<IAdapter>();

            builder.RegisterType<ConsoleLogger>()
                .Keyed<ILogger>("Solution");

            builder.RegisterType<FileLogger>()
                .Keyed<ILogger>("Other");

            builder.RegisterType<SolutionExplorerMixed>()
                .WithAttributeFilter();

            var container = builder.Build();

            var allAdapters = container.Resolve<IEnumerable<IAdapter>>().Count();
            Assert.Equal(3, allAdapters);

            var explorer = container.Resolve<SolutionExplorerMixed>();
            Assert.Equal(2, explorer.Adapters.Count);
            Assert.IsType<ConsoleLogger>(explorer.Logger);
        }

        [Fact]
        public void KeyFilterIsAppliedOnConstructorDependencySingle()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<ConsoleLogger>()
                .Keyed<ILogger>("Solution");

            builder.RegisterType<FileLogger>()
                .Keyed<ILogger>("Other");

            builder.RegisterType<SolutionExplorerKeyed>()
                .WithAttributeFilter();

            var container = builder.Build();
            var explorer = container.Resolve<SolutionExplorerKeyed>();

            Assert.IsType<ConsoleLogger>(explorer.Logger);
        }

        [Fact]
        public void KeyFilterIsAppliedOnConstructorDependencyMultiple()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<MsBuildAdapter>()
                .Keyed<IAdapter>("Solution");

            builder.RegisterType<DteAdapter>()
                .Keyed<IAdapter>("Solution");

            builder.RegisterType<ToolWindowAdapter>()
                .Keyed<IAdapter>("Other");

            builder.RegisterType<SolutionExplorerKeyed>()
                .WithAttributeFilter();

            var container = builder.Build();

            var otherAdapters = container.ResolveKeyed<IEnumerable<IAdapter>>("Other").Count();

            Assert.Equal(1, otherAdapters);

            var explorer = container.Resolve<SolutionExplorerKeyed>();

            Assert.Equal(2, explorer.Adapters.Count);
        }

        [Fact]
        public void KeyFilterIsAppliedOnConstructorDependencySingleWithLazy()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ConsoleLogger>()
                .Keyed<ILogger>("Manager");

            builder.RegisterType<FileLogger>()
                .Keyed<ILogger>("Other");

            builder.RegisterType<ManagerWithLazySingle>()
                .WithAttributeFilter();

            var container = builder.Build();
            var resolved = container.Resolve<ManagerWithLazySingle>();

            Assert.IsType<ConsoleLogger>(resolved.Logger.Value);
        }

        [Fact]
        public void KeyFilterIsAppliedOnConstructorDependencySingleWithMeta()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<ConsoleLogger>()
                .Keyed<ILogger>("Manager");

            builder.RegisterType<FileLogger>()
                .Keyed<ILogger>("Other");

            builder.RegisterType<ManagerWithMetaSingle>()
                .WithAttributeFilter();

            var container = builder.Build();

            var resolved = container.Resolve<ManagerWithMetaSingle>();

            Assert.IsType<ConsoleLogger>(resolved.Logger.Value);
        }

        [Fact]
        public void KeyFilterIsAppliedOnConstructorDependencySingleWithOwned()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<ConsoleLogger>()
                .Keyed<ILogger>("Manager");

            builder.RegisterType<FileLogger>()
                .Keyed<ILogger>("Other");

            builder.RegisterType<ManagerWithOwnedSingle>()
                .WithAttributeFilter();

            var container = builder.Build();

            var resolved = container.Resolve<ManagerWithOwnedSingle>();

            Assert.IsType<ConsoleLogger>(resolved.Logger.Value);
        }

        [Fact]
        public void KeyFilterIsAppliedOnConstructorDependencyManyWithLazy()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<ConsoleLogger>()
                .Keyed<ILogger>("Manager");

            builder.RegisterType<SqlLogger>()
                .Keyed<ILogger>("Manager");

            builder.RegisterType<FileLogger>()
                .Keyed<ILogger>("Other");

            builder.RegisterType<ManagerWithLazyMany>()
                .WithAttributeFilter();

            var container = builder.Build();

            var resolved = container.Resolve<ManagerWithLazyMany>();

            Assert.Equal(2, resolved.Loggers.Count());
        }

        [Fact]
        public void KeyFilterIsAppliedOnConstructorDependencyManyWithMeta()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<ConsoleLogger>()
                .Keyed<ILogger>("Manager");

            builder.RegisterType<SqlLogger>()
                .Keyed<ILogger>("Manager");

            builder.RegisterType<FileLogger>()
                .Keyed<ILogger>("Other");

            builder.RegisterType<ManagerWithMetaMany>()
                .WithAttributeFilter();

            var container = builder.Build();

            var resolved = container.Resolve<ManagerWithMetaMany>();

            Assert.Equal(2, resolved.Loggers.Count());
        }

        [Fact]
        public void KeyFilterIsAppliedOnConstructorDependencyManyWithOwned()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ConsoleLogger>()
                .Keyed<ILogger>("Manager");

            builder.RegisterType<SqlLogger>()
                .Keyed<ILogger>("Manager");

            builder.RegisterType<FileLogger>()
                .Keyed<ILogger>("Other");

            builder.RegisterType<ManagerWithOwnedMany>()
                .WithAttributeFilter();

            var container = builder.Build();

            var resolved = container.Resolve<ManagerWithOwnedMany>();

            Assert.Equal(2, resolved.Loggers.Count());
        }

        [Fact]
        public void MetadataFilterIsAppliedOnConstructorDependencySingle()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<ConsoleLogger>()
                .WithMetadata("LoggerName", "Solution")
                .As<ILogger>();

            builder.RegisterType<FileLogger>()
                .WithMetadata("LoggerName", "Other")
                .As<ILogger>();

            builder.RegisterType<SolutionExplorerMetadata>()
                .WithAttributeFilter();

            var container = builder.Build();

            var explorer = container.Resolve<SolutionExplorerMetadata>();

            Assert.IsType<ConsoleLogger>(explorer.Logger);
        }

        [Fact]
        public void MetadataFilterIsAppliedOnConstructorDependencyMultiple()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<MsBuildAdapter>()
                .WithMetadata<AdapterMetadata>(m => m.For(am => am.Target, "Solution"))
                .As<IAdapter>();

            builder.RegisterType<DteAdapter>()
                .WithMetadata<AdapterMetadata>(m => m.For(am => am.Target, "Solution"))
                .As<IAdapter>();

            builder.RegisterType<ToolWindowAdapter>()
                .WithMetadata<AdapterMetadata>(m => m.For(am => am.Target, "Other"))
                .As<IAdapter>();

            builder.RegisterType<SolutionExplorerMetadata>().WithAttributeFilter();

            var container = builder.Build();

            var allAdapters = container.Resolve<IEnumerable<IAdapter>>().Count();

            Assert.Equal(3, allAdapters);

            var explorer = container.Resolve<SolutionExplorerMetadata>();

            Assert.Equal(2, explorer.Adapters.Count);
        }

        [Fact]
        public void ComponentsThatAreNotUsedDoNotGetActivated()
        {
            int adapterActivationCount = 0;
            var builder = new ContainerBuilder();
            builder.RegisterType<MsBuildAdapter>()
                .WithMetadata<AdapterMetadata>(m => m.For(am => am.Target, "Solution"))
                .As<IAdapter>()
                .OnActivating(h => adapterActivationCount++);

            builder.RegisterType<DteAdapter>()
                .WithMetadata<AdapterMetadata>(m => m.For(am => am.Target, "Solution"))
                .As<IAdapter>()
                .OnActivating(h => adapterActivationCount++);

            builder.RegisterType<ToolWindowAdapter>()
                .WithMetadata<AdapterMetadata>(m => m.For(am => am.Target, "Other"))
                .As<IAdapter>()
                .OnActivating(h => adapterActivationCount++);

            builder.RegisterType<SolutionExplorerMetadata>()
                .WithAttributeFilter();

            var container = builder.Build();

            container.Resolve<SolutionExplorerMetadata>();

            Assert.Equal(2, adapterActivationCount);
        }

        public interface ILogger
        {
        }

        public class ConsoleLogger : ILogger
        {
        }

        public class FileLogger : ILogger
        {
        }

        public class SqlLogger : ILogger
        {
        }

        public interface IAdapter
        {
        }

        public class MsBuildAdapter : IAdapter
        {
        }

        public class DteAdapter : IAdapter
        {
        }

        public class ToolWindowAdapter : IAdapter
        {
        }

        public class ManagerWithLazySingle
        {
            public ManagerWithLazySingle([WithKey("Manager")] Lazy<ILogger> logger)
            {
                Logger = logger;
            }

            public Lazy<ILogger> Logger { get; set; }
        }

        public class ManagerWithMetaSingle
        {
            public ManagerWithMetaSingle([WithKey("Manager")] Meta<ILogger, EmptyMetadata> logger)
            {
                Logger = logger;
            }

            public Meta<ILogger, EmptyMetadata> Logger { get; set; }
        }

        public class ManagerWithOwnedSingle
        {
            public ManagerWithOwnedSingle([WithKey("Manager")] Owned<ILogger> logger)
            {
                Logger = logger;
            }

            public Owned<ILogger> Logger { get; set; }
        }

        public class ManagerWithLazyMany
        {
            public ManagerWithLazyMany([WithKey("Manager")] IEnumerable<Lazy<ILogger>> loggers)
            {
                Loggers = loggers;
            }

            public IEnumerable<Lazy<ILogger>> Loggers { get; set; }
        }

        public class ManagerWithMetaMany
        {
            public ManagerWithMetaMany([WithKey("Manager")] IEnumerable<Meta<ILogger, EmptyMetadata>> loggers)
            {
                Loggers = loggers;
            }

            public IEnumerable<Meta<ILogger, EmptyMetadata>> Loggers { get; set; }
        }

        public class ManagerWithOwnedMany
        {
            public ManagerWithOwnedMany([WithKey("Manager")] IEnumerable<Owned<ILogger>> loggers)
            {
                Loggers = loggers;
            }

            public IEnumerable<Owned<ILogger>> Loggers { get; set; }
        }

        public class SolutionExplorerKeyed
        {
            public SolutionExplorerKeyed(
            [WithKey("Solution")] IEnumerable<IAdapter> adapters,
            [WithKey("Solution")] ILogger logger)
            {
                Adapters = adapters.ToList();
                Logger = logger;
            }

            public List<IAdapter> Adapters { get; set; }

            public ILogger Logger { get; set; }
        }

        public class SolutionExplorerMetadata
        {
            public SolutionExplorerMetadata(
            [WithMetadata("Target", "Solution")] IEnumerable<IAdapter> adapters,
            [WithMetadata("LoggerName", "Solution")] ILogger logger)
            {
                Adapters = adapters.ToList();
                Logger = logger;
            }

            public List<IAdapter> Adapters { get; set; }

            public ILogger Logger { get; set; }
        }

        public class SolutionExplorerMixed
        {
            public SolutionExplorerMixed(
            [WithMetadata("Target", "Solution")] IEnumerable<IAdapter> adapters,
            [WithKey("Solution")] ILogger logger)
            {
                Adapters = adapters.ToList();
                Logger = logger;
            }

            public List<IAdapter> Adapters { get; set; }

            public ILogger Logger { get; set; }
        }

        public class AdapterMetadata
        {
            public string Target { get; set; }
        }

        public class EmptyMetadata
        {
        }
    }
}