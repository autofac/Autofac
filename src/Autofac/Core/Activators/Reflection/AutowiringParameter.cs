// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Core.Resolving.Pipeline;
using Autofac.Util;

namespace Autofac.Core.Activators.Reflection;

/// <summary>
/// Supplies values based on the target parameter type.
/// </summary>
public class AutowiringParameter : Parameter
{
    /// <summary>
    /// Returns true if the parameter is able to provide a value to a particular site.
    /// </summary>
    /// <param name="pi">Constructor, method, or property-mutator parameter.</param>
    /// <param name="context">The component context in which the value is being provided.</param>
    /// <param name="valueProvider">If the result is true, the valueProvider parameter will
    /// be set to a function that will lazily retrieve the parameter value. If the result is false,
    /// will be set to null.</param>
    /// <returns>True if a value can be supplied; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="pi" /> or <paramref name="context" /> is <see langword="null" />.
    /// </exception>
    public override bool CanSupplyValue(ParameterInfo pi, IComponentContext context, [NotNullWhen(returnValue: true)] out Func<object?>? valueProvider)
    {
        if (pi == null)
        {
            throw new ArgumentNullException(nameof(pi));
        }

        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (TryGetServiceRegistration(pi, context, typedService: null, out var service, out var implementation))
        {
            valueProvider = CreateValueProvider(context, service, implementation);
            return true;
        }

        valueProvider = null;
        return false;
    }

    /// <summary>
    /// Finds the registration that supplies an autowired value for a parameter. Shared by <see cref="CanSupplyValue"/>
    /// and <see cref="ConstructorBinder"/>, which binds autowired constructor arguments without creating a delegate.
    /// </summary>
    /// <param name="pi">The parameter to supply.</param>
    /// <param name="context">The component context in which the value is being provided.</param>
    /// <param name="typedService">A cached <see cref="TypedService"/> for the parameter type, or <see langword="null"/> to create one.</param>
    /// <param name="service">The service that supplies the value.</param>
    /// <param name="implementation">The registration for <paramref name="service"/>, if found.</param>
    /// <returns>True if a registration was found; otherwise, false.</returns>
    internal static bool TryGetServiceRegistration(ParameterInfo pi, IComponentContext context, TypedService? typedService, out Service service, out ServiceRegistration implementation)
    {
        if (context is ResolveRequestContext ctx
            && (ctx.Registration.Options & Registration.RegistrationOptions.Composite) == Registration.RegistrationOptions.Composite
            && ctx.Service is KeyedService keyedService
            && pi.ParameterType.IsGenericEnumerableInterfaceType()
            && pi.ParameterType.IsGenericTypeContainingType(keyedService.ServiceType))
        {
            service = new KeyedService(keyedService.ServiceKey, pi.ParameterType);
        }
        else
        {
            service = typedService ?? new TypedService(pi.ParameterType);
        }

        return context.ComponentRegistry.TryGetServiceRegistration(service, out implementation);
    }

    /// <summary>
    /// Resolves an autowired value from a registration found by <see cref="TryGetServiceRegistration"/>.
    /// </summary>
    /// <param name="context">The component context in which the value is being provided.</param>
    /// <param name="service">The service that supplies the value.</param>
    /// <param name="implementation">The registration for <paramref name="service"/>.</param>
    /// <returns>The resolved value.</returns>
    internal static object ResolveValue(IComponentContext context, Service service, ServiceRegistration implementation)
    {
        return context.ResolveComponent(new ResolveRequest(service, implementation, Enumerable.Empty<Parameter>()));
    }

    // Kept separate from CanSupplyValue: a lambda capturing parameters allocates its closure on method entry,
    // which would otherwise cost every call an allocation, including those that cannot supply a value.
    private static Func<object?> CreateValueProvider(IComponentContext context, Service service, ServiceRegistration implementation)
    {
        return () => ResolveValue(context, service, implementation);
    }
}
