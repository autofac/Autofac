// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;
using Autofac.Core;

namespace Autofac.Test.Core;

public class KeyedServiceParameterInjectorTests
{
    [Fact]
    public void AddKeyedServiceParameter_ServiceOverload_NullService()
    {
        Assert.Throws<ArgumentNullException>(
            () => KeyedServiceParameterInjector.AddKeyedServiceParameter(
                service: null!,
                parameters: ResolveRequest.NoParameters));
    }

    [Fact]
    public void AddKeyedServiceParameter_ServiceOverload_NullNonKeyedServiceParameters()
    {
        Assert.Throws<ArgumentNullException>(
            () => KeyedServiceParameterInjector.AddKeyedServiceParameter(
                service: new TypedService(typeof(object)),
                parameters: null!));
    }

    [Fact]
    public void AddKeyedServiceParameter_ServiceRegistrationOverload_NullService()
    {
        Assert.Throws<ArgumentNullException>(
            () => KeyedServiceParameterInjector.AddKeyedServiceParameter(
                service: null!,
                parameters: ResolveRequest.NoParameters,
                registration: null));
    }

    [Fact]
    public void AddKeyedServiceParameter_ServiceRegistrationOverload_NullParameters()
    {
        Assert.Throws<ArgumentNullException>(
            () => KeyedServiceParameterInjector.AddKeyedServiceParameter(
                service: new KeyedService("key", typeof(object)),
                parameters: null!,
                registration: null));
    }

    [Fact]
    public void AddKeyedServiceParameter_ServiceKeyOverload_NullServiceKey()
    {
        Assert.Throws<ArgumentNullException>(
            () => KeyedServiceParameterInjector.AddKeyedServiceParameter(
                serviceKey: null!,
                parameters: ResolveRequest.NoParameters));
    }

    [Fact]
    public void AddKeyedServiceParameter_ServiceKeyOverload_NullParameters()
    {
        Assert.Throws<ArgumentNullException>(
            () => KeyedServiceParameterInjector.AddKeyedServiceParameter(
                serviceKey: "key",
                parameters: null!));
    }

    [Fact]
    public void AddKeyedServiceParameter_SkipsWhenServiceIsAnyKey()
    {
        var parameters = KeyedServiceParameterInjector.AddKeyedServiceParameter(
            new KeyedService(KeyedService.AnyKey, typeof(object)),
            ResolveRequest.NoParameters);

        Assert.Same(ResolveRequest.NoParameters, parameters);
    }

    [Fact]
    public void AddKeyedServiceParameter_SkipsWhenActivatorDoesNotNeedKey()
    {
        using var registration = CreateRegistration<PlainService>();
        var parameters = KeyedServiceParameterInjector.AddKeyedServiceParameter(
            new KeyedService("key", typeof(PlainService)),
            ResolveRequest.NoParameters,
            registration);

        Assert.Same(ResolveRequest.NoParameters, parameters);
    }

    [Fact]
    public void AddKeyedServiceParameter_AppendsWhenActivatorNeedsKey()
    {
        using var registration = CreateRegistration<NeedsConstructorKey>();
        var parameters = KeyedServiceParameterInjector.AddKeyedServiceParameter(
            new KeyedService("key", typeof(NeedsConstructorKey)),
            ResolveRequest.NoParameters,
            registration).ToArray();

        var keyParameter = Assert.Single(parameters);
        Assert.IsType<KeyedServiceKeyParameter>(keyParameter);
        Assert.Equal("key", ((KeyedServiceKeyParameter)keyParameter).ServiceKey);
    }

    private static IComponentRegistration CreateRegistration<T>()
        => RegistrationBuilder
            .ForType<T>()
            .CreateRegistration();

    private sealed class PlainService
    {
    }

    private sealed class NeedsConstructorKey
    {
        public NeedsConstructorKey([ServiceKey] object key)
        {
            Key = key;
        }

        public object Key { get; }
    }
}
