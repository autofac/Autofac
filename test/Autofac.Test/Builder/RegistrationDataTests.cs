// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;
using Autofac.Core;

namespace Autofac.Test.Builder;

public class RegistrationDataTests
{
    private static readonly Service _defaultService = new TypedService(typeof(object));

    private static readonly Service _a = new TypedService(typeof(string));

    private static readonly Service _b = new KeyedService("b", typeof(string));

    private static readonly Service _c = new TypedService(typeof(int));

    private static readonly Service _d = new KeyedService("d", typeof(int));

    [Fact]
    public void DefaultServiceIsUsedUntilAServiceIsAdded()
    {
        var data = new RegistrationData(_defaultService);
        AssertServices(data, _defaultService);

        data.AddService(_a);
        AssertServices(data, _a);
    }

    [Fact]
    public void AutoActivateServiceDoesNotReplaceTheDefaultService()
    {
        var data = new RegistrationData(_defaultService);
        var autoActivate = new AutoActivateService();

        data.AddService(autoActivate);
        AssertServices(data, _defaultService, autoActivate);

        data.AddService(_a);
        AssertServices(data, autoActivate, _a);
    }

    [Fact]
    public void DefaultServiceAlreadyInTheServicesIsNotRepeated()
    {
        var data = new RegistrationData(new AutoActivateService());
        var autoActivate = new AutoActivateService();

        data.AddService(autoActivate);

        AssertServices(data, autoActivate);
    }

    [Fact]
    public void ServicesAreDistinctAndKeepTheOrderTheyWereAddedIn()
    {
        var data = new RegistrationData(_defaultService);

        data.AddService(_a);
        data.AddService(_b);
        data.AddService(new TypedService(typeof(string)));
        data.AddService(_c);
        data.AddService(new KeyedService("b", typeof(string)));
        data.AddService(_d);

        AssertServices(data, _a, _b, _c, _d);
    }

    [Fact]
    public void ServicesCanBeAddedAgainAfterClearing()
    {
        var data = new RegistrationData(_defaultService);
        data.AddService(_a);
        data.AddService(_b);

        data.ClearServices();
        AssertServices(data);

        data.AddService(_b);
        data.AddService(_a);
        data.AddService(_b);
        AssertServices(data, _b, _a);
    }

    [Fact]
    public void ClearingASingleServiceLeavesNoServices()
    {
        var data = new RegistrationData(_defaultService);
        data.AddService(_a);

        data.ClearServices();

        AssertServices(data);
    }

    [Fact]
    public void EmptyAddServicesClearsTheDefaultService()
    {
        var data = new RegistrationData(_defaultService);

        data.AddServices(Array.Empty<Service>());

        AssertServices(data);
    }

    [Fact]
    public void CopyFromAddsTheServicesNotAlreadyPresent()
    {
        var data = new RegistrationData(_defaultService);
        data.AddService(_a);
        data.AddService(_b);

        var other = new RegistrationData(_defaultService);
        other.AddService(_b);
        other.AddService(_c);
        other.AddService(_a);
        other.AddService(_d);

        data.CopyFrom(other, includeDefaultService: false);

        AssertServices(data, _a, _b, _c, _d);
    }

    [Fact]
    public void CopyFromIntoEmptyDataKeepsTheOrderOfTheOther()
    {
        var data = new RegistrationData(_defaultService);

        var other = new RegistrationData(_a);
        other.AddService(_c);
        other.AddService(_b);

        data.CopyFrom(other, includeDefaultService: true);

        AssertServices(data, _c, _b);
    }

    [Fact]
    public void ServiceWithoutAHashCodeIsRejected()
    {
        // Service.GetHashCode throws unless overridden; the set computed it for every service
        // added, including the first.
        var data = new RegistrationData(_defaultService);

        Assert.Throws<NotImplementedException>(() => data.AddService(new ServiceWithoutHashCode()));
    }

    private static void AssertServices(RegistrationData data, params Service[] expected)
    {
        Assert.Equal(expected, data.Services);
        Assert.Equal(expected, data.ServicesToArray());
    }

    private sealed class ServiceWithoutHashCode : Service
    {
        public override string Description => "no hash code";

        public override bool Equals(object? obj) => ReferenceEquals(this, obj);

        public override int GetHashCode() => base.GetHashCode();
    }
}
