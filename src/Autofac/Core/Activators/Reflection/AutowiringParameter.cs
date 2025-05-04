// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Core.Resolving.Pipeline;

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

        Service service = new TypedService(pi.ParameterType);

        if (context is DefaultResolveRequestContext ctx)
        {
            if ((ctx.Registration.Options & Registration.RegistrationOptions.Composite) == Registration.RegistrationOptions.Composite
                && ctx.Service is KeyedService keyedService
                && pi.ParameterType.IsGenericType
                && pi.ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                && pi.ParameterType.GenericTypeArguments[0] == keyedService.ServiceType)
            {
                service = new KeyedService(keyedService.ServiceKey, pi.ParameterType);
            }
        }

        if (context.ComponentRegistry.TryGetServiceRegistration(service, out var implementation))
        {
            valueProvider = () => context.ResolveComponent(new ResolveRequest(service, implementation, Enumerable.Empty<Parameter>()));
            return true;
        }

        valueProvider = null;
        return false;
    }
}
