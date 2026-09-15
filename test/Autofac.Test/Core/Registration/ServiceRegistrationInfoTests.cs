// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Test.Scenarios.RegistrationSources;

namespace Autofac.Test.Core.Registration;

public sealed class ServiceRegistrationInfoTests
{
    [Fact]
    public void IsSourceQueuedIsFalseBeforeInitializationBegins()
    {
        var info = new ServiceRegistrationInfo(new TypedService(typeof(object)));

        Assert.False(info.IsSourceQueued(new ObjectRegistrationSource()));
    }

    [Fact]
    public void IsSourceQueuedIsTrueWhileTheSourceIsStillWaiting()
    {
        var source = new ObjectRegistrationSource();
        var info = new ServiceRegistrationInfo(new TypedService(typeof(object)));

        info.BeginInitialization(new IRegistrationSource[] { source });

        Assert.True(info.IsSourceQueued(source));
    }

    [Fact]
    public void IsSourceQueuedIsFalseOnceTheSourceHasBeenDequeued()
    {
        var source = new ObjectRegistrationSource();
        var info = new ServiceRegistrationInfo(new TypedService(typeof(object)));

        info.BeginInitialization(new IRegistrationSource[] { source });
        info.DequeueNextSource();

        Assert.False(info.IsSourceQueued(source));
    }

    [Fact]
    public void IsSourceQueuedIsFalseForASourceThisServiceNeverHad()
    {
        var info = new ServiceRegistrationInfo(new TypedService(typeof(object)));

        info.BeginInitialization(new IRegistrationSource[] { new ObjectRegistrationSource() });

        Assert.False(info.IsSourceQueued(new ObjectRegistrationSource()));
    }

    [Fact]
    public void IsSourceQueuedIsFalseAfterInitializationCompletes()
    {
        var source = new ObjectRegistrationSource();
        var info = new ServiceRegistrationInfo(new TypedService(typeof(object)));

        info.BeginInitialization(new IRegistrationSource[] { source });
        info.CompleteInitialization();

        Assert.False(info.IsSourceQueued(source));
    }
}
