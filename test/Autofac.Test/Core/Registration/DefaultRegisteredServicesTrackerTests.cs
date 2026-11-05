// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Registration;

namespace Autofac.Test.Core.Registration;

public class DefaultRegisteredServicesTrackerTests
{
    [Fact]
    public void RegistrationsAddedBeforeAndAfterCompletionAreEnumeratedInOrder()
    {
        using var tracker = new DefaultRegisteredServicesTracker();
        var first = Registration();
        var second = Registration();
        var third = Registration();

        tracker.AddRegistration(first, preserveDefaults: false);
        tracker.AddRegistration(second, preserveDefaults: false);
        tracker.Complete();
        tracker.AddRegistration(third, preserveDefaults: false);

        Assert.Equal(new[] { first, second, third }, tracker.Registrations);
    }

    [Fact]
    public void RegistrationAddedWhileEnumeratingIsLeftOutOfThatEnumeration()
    {
        using var tracker = new DefaultRegisteredServicesTracker();
        var first = Registration();
        var second = Registration();
        tracker.AddRegistration(first, preserveDefaults: false);

        var seen = new List<IComponentRegistration>();
        foreach (var registration in tracker.Registrations)
        {
            seen.Add(registration);
            if (seen.Count == 1)
            {
                tracker.AddRegistration(second, preserveDefaults: false);
            }
        }

        Assert.Equal(new[] { first }, seen);
        Assert.Equal(new[] { first, second }, tracker.Registrations);
    }

    private static IComponentRegistration Registration()
        => RegistrationBuilder.ForType<object>().CreateRegistration();
}
