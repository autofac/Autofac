// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;

namespace Autofac.Test.Builder;

public class StartableManagerTests
{
    private const string StartOnActivate = MetadataKeys.StartOnActivatePropertyKey;

    [Fact]
    public void ChildScope_NothingToStart()
    {
        using var container = new ContainerBuilder().Build();

        using var scope = container.BeginLifetimeScope(b => b.RegisterType<object>());

        Assert.False(scope.ComponentRegistry.Properties.ContainsKey(StartOnActivate));
    }

    [Fact]
    public void Container_StartableStarted()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<RecordingStartable>().As<IStartable>().AsSelf().SingleInstance();

        using var container = builder.Build();

        Assert.True(container.Resolve<RecordingStartable>().FlagSetWhenStarted);
        Assert.False(container.ComponentRegistry.Properties.ContainsKey(StartOnActivate));
    }

    [Fact]
    public void ChildScope_StartableStarted()
    {
        using var container = new ContainerBuilder().Build();

        using var scope = container.BeginLifetimeScope(b => b.RegisterType<RecordingStartable>().As<IStartable>().AsSelf().SingleInstance());

        Assert.True(scope.Resolve<RecordingStartable>().FlagSetWhenStarted);
        Assert.False(scope.ComponentRegistry.Properties.ContainsKey(StartOnActivate));
    }

    [Fact]
    public void Container_OnlyAutoActivated()
    {
        bool? flagSetWhenActivated = null;
        var builder = new ContainerBuilder();
        builder.RegisterType<object>()
            .AutoActivate()
            .OnActivated(e => flagSetWhenActivated = e.Context.ComponentRegistry.Properties.ContainsKey(StartOnActivate));

        using var container = builder.Build();

        Assert.True(flagSetWhenActivated);
        Assert.False(container.ComponentRegistry.Properties.ContainsKey(StartOnActivate));
    }

    private sealed class RecordingStartable : IStartable
    {
        private readonly IComponentContext _context;

        public RecordingStartable(IComponentContext context)
        {
            _context = context;
        }

        public bool FlagSetWhenStarted
        {
            get; private set;
        }

        public void Start() => FlagSetWhenStarted = _context.ComponentRegistry.Properties.ContainsKey(StartOnActivate);
    }
}
