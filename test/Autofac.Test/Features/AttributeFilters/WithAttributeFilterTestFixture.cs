// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;
using Autofac.Features.AttributeFilters;
using Autofac.Features.Metadata;
using Autofac.Features.OwnedInstances;

namespace Autofac.Test.Features.AttributeFilters;

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
            .WithAttributeFiltering();

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
            .WithAttributeFiltering();

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

        builder.RegisterType<ConsoleLogger>()
            .Keyed<ILogger>("Solution");

        builder.RegisterType<SolutionExplorerKeyed>()
            .WithAttributeFiltering();

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
            .WithAttributeFiltering();

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
            .WithAttributeFiltering();

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
            .WithAttributeFiltering();

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
            .WithAttributeFiltering();

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
            .WithAttributeFiltering();

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
            .WithAttributeFiltering();

        var container = builder.Build();

        var resolved = container.Resolve<ManagerWithOwnedMany>();

        Assert.Equal(2, resolved.Loggers.Count());
    }

    [Fact]
    public void KeyFilterIsAppliedOnMultipleIndividualConcreteDependencies()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<ConsoleLogger>().As<ILogger>();
        builder.Register(ctx => new IdentifiableObject { Id = "a" }).Keyed<IdentifiableObject>("First");
        builder.Register(ctx => new IdentifiableObject { Id = "b" }).Keyed<IdentifiableObject>("Second");
        builder.Register(ctx => new IdentifiableObject { Id = "c" }).Keyed<IdentifiableObject>("Third");
        builder.RegisterType<ManagerWithManyIndividualConcrete>().WithAttributeFiltering();

        var container = builder.Build();
        var resolved = container.Resolve<ManagerWithManyIndividualConcrete>();

        Assert.IsType<ConsoleLogger>(resolved.Logger);
        Assert.Equal("a", resolved.First.Id);
        Assert.Equal("b", resolved.Second.Id);
        Assert.Equal("c", resolved.Third.Id);
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
            .WithAttributeFiltering();

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

        builder.RegisterType<ConsoleLogger>()
            .WithMetadata("LoggerName", "Solution")
            .As<ILogger>();

        builder.RegisterType<SolutionExplorerMetadata>().WithAttributeFiltering();

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

        builder.RegisterType<ConsoleLogger>()
            .WithMetadata("LoggerName", "Solution")
            .As<ILogger>();

        builder.RegisterType<SolutionExplorerMetadata>()
            .WithAttributeFiltering();

        var container = builder.Build();

        container.Resolve<SolutionExplorerMetadata>();

        Assert.Equal(2, adapterActivationCount);
    }

    [Fact]
    public void RequiredParameterWithKeyFilterCanNotBeResolvedWhenNotRegister()
    {
        var builder = new ContainerBuilder();

        builder.RegisterType<RequiredParameterWithKeyFilter>()
          .WithAttributeFiltering()
          .As<RequiredParameterWithKeyFilter>();

        Assert.Throws<DependencyResolutionException>(() => builder.Build().Resolve<RequiredParameterWithKeyFilter>());
    }

    [Fact]
    public void OptionalParameterWithKeyFilterCanBeResolvedBySpecifiedDefaultValue()
    {
        var builder = new ContainerBuilder();

        builder.RegisterType<OptionalParameterWithKeyFilter>()
          .WithAttributeFiltering()
          .As<OptionalParameterWithKeyFilter>();

        var instance = builder.Build().Resolve<OptionalParameterWithKeyFilter>();

        Assert.Equal(15, instance.Parameter);
    }

    private interface ILogger
    {
    }

    private class ConsoleLogger : ILogger
    {
    }

    private class FileLogger : ILogger
    {
    }

    private class SqlLogger : ILogger
    {
    }

    private interface IAdapter
    {
    }

    private class MsBuildAdapter : IAdapter
    {
    }

    private class DteAdapter : IAdapter
    {
    }

    private class ToolWindowAdapter : IAdapter
    {
    }

    private class IdentifiableObject
    {
        public string Id { get; set; }
    }

    private class ManagerWithManyIndividualConcrete
    {
        public ManagerWithManyIndividualConcrete(
            ILogger logger,
            [KeyFilter("First")] IdentifiableObject first,
            [KeyFilter("Second")] IdentifiableObject second,
            [KeyFilter("Third")] IdentifiableObject third)
        {
            Logger = logger;
            First = first;
            Second = second;
            Third = third;
        }

        public ILogger Logger { get; set; }

        public IdentifiableObject First { get; set; }

        public IdentifiableObject Second { get; set; }

        public IdentifiableObject Third { get; set; }
    }

    private class ManagerWithLazySingle
    {
        public ManagerWithLazySingle([KeyFilter("Manager")] Lazy<ILogger> logger)
        {
            Logger = logger;
        }

        public Lazy<ILogger> Logger { get; set; }
    }

    private class ManagerWithMetaSingle
    {
        public ManagerWithMetaSingle([KeyFilter("Manager")] Meta<ILogger, EmptyMetadata> logger)
        {
            Logger = logger;
        }

        public Meta<ILogger, EmptyMetadata> Logger { get; set; }
    }

    private class ManagerWithOwnedSingle
    {
        public ManagerWithOwnedSingle([KeyFilter("Manager")] Owned<ILogger> logger)
        {
            Logger = logger;
        }

        public Owned<ILogger> Logger { get; set; }
    }

    private class ManagerWithLazyMany
    {
        public ManagerWithLazyMany([KeyFilter("Manager")] IEnumerable<Lazy<ILogger>> loggers)
        {
            Loggers = loggers;
        }

        public IEnumerable<Lazy<ILogger>> Loggers { get; set; }
    }

    private class ManagerWithMetaMany
    {
        public ManagerWithMetaMany([KeyFilter("Manager")] IEnumerable<Meta<ILogger, EmptyMetadata>> loggers)
        {
            Loggers = loggers;
        }

        public IEnumerable<Meta<ILogger, EmptyMetadata>> Loggers { get; set; }
    }

    private class ManagerWithOwnedMany
    {
        public ManagerWithOwnedMany([KeyFilter("Manager")] IEnumerable<Owned<ILogger>> loggers)
        {
            Loggers = loggers;
        }

        public IEnumerable<Owned<ILogger>> Loggers { get; set; }
    }

    private class SolutionExplorerKeyed
    {
        public SolutionExplorerKeyed(
        [KeyFilter("Solution")] IEnumerable<IAdapter> adapters,
        [KeyFilter("Solution")] ILogger logger)
        {
            Adapters = adapters.ToList();
            Logger = logger;
        }

        public List<IAdapter> Adapters { get; set; }

        public ILogger Logger { get; set; }
    }

    private class SolutionExplorerMetadata
    {
        public SolutionExplorerMetadata(
        [MetadataFilter("Target", "Solution")] IEnumerable<IAdapter> adapters,
        [MetadataFilter("LoggerName", "Solution")] ILogger logger)
        {
            Adapters = adapters.ToList();
            Logger = logger;
        }

        public List<IAdapter> Adapters { get; set; }

        public ILogger Logger { get; set; }
    }

    private class SolutionExplorerMixed
    {
        public SolutionExplorerMixed(
        [MetadataFilter("Target", "Solution")] IEnumerable<IAdapter> adapters,
        [KeyFilter("Solution")] ILogger logger)
        {
            Adapters = adapters.ToList();
            Logger = logger;
        }

        public List<IAdapter> Adapters { get; set; }

        public ILogger Logger { get; set; }
    }

    private class AdapterMetadata
    {
        public string Target { get; set; }
    }

    private class EmptyMetadata
    {
    }

    private class RequiredParameterWithKeyFilter
    {
        public int Parameter { get; set; }

        public RequiredParameterWithKeyFilter([KeyFilter(0)] int parameter)
        {
            Parameter = parameter;
        }
    }

    private class OptionalParameterWithKeyFilter
    {
        public int Parameter { get; set; }

        public OptionalParameterWithKeyFilter([KeyFilter(0)] int parameter = 15)
        {
            Parameter = parameter;
        }
    }
}
