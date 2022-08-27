// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Reflection;
using Autofac.Util;

namespace Autofac.Core.Activators.Reflection;

/// <summary>
/// Provide helper methods for injecting property values.
/// </summary>
internal static class AutowiringPropertyInjector
{
    /// <summary>
    /// Name of the parameter containing the instance type provided when resolving an injected service.
    /// </summary>
    internal const string InstanceTypeNamedParameter = "Autofac.AutowiringPropertyInjector.InstanceType";

    private static readonly MethodInfo CallPropertySetterOpenGenericMethod =
        typeof(AutowiringPropertyInjector).GetDeclaredMethod(nameof(CallPropertySetter));

    /// <summary>
    /// Inject properties onto an instance, filtered by a property selector.
    /// </summary>
    /// <param name="context">The component context to resolve dependencies from.</param>
    /// <param name="instance">The instance to inject onto.</param>
    /// <param name="propertySelector">The property selector.</param>
    /// <param name="parameters">The set of parameters for the resolve that can be used to satisfy injectable properties.</param>
    public static void InjectProperties(IComponentContext context, object instance, IPropertySelector propertySelector, IEnumerable<Parameter> parameters)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (instance == null)
        {
            throw new ArgumentNullException(nameof(instance));
        }

        if (propertySelector == null)
        {
            throw new ArgumentNullException(nameof(propertySelector));
        }

        if (parameters == null)
        {
            throw new ArgumentNullException(nameof(parameters));
        }

        var resolveParameters = parameters as Parameter[] ?? parameters.ToArray();

        var injectablePropertiesCache = ReflectionCache.Shared.Internal.AutowiringInjectableProperties;

        var instanceType = instance.GetType();
        var injectableProperties = injectablePropertiesCache.GetOrAdd(instanceType, type => GetInjectableProperties(type).ToList());

        for (var index = 0; index < injectableProperties.Count; index++)
        {
            var property = injectableProperties[index];

            if (!propertySelector.InjectProperty(property, instance))
            {
                continue;
            }

            // SetMethod will be non-null if GetInjectableProperties included it.
            var setParameter = property.SetMethod!.GetParameters()[0];
            var valueProvider = (Func<object?>?)null;

            // Issue #1275: If a delegate factory has a named parameter 'value' it will try to
            // inject into any property being autowired.
            var parameter = resolveParameters.FirstOrDefault(p =>
                p.CanSupplyValue(setParameter, context, out valueProvider) &&
                !(p is NamedParameter n && n.Name.Equals("value", StringComparison.Ordinal)));
            if (parameter != null)
            {
                var setter = ReflectionCache.Shared.Internal.AutowiringPropertySetters.GetOrAdd(property, MakeFastPropertySetter);
                setter(instance, valueProvider!());
                continue;
            }

            var propertyService = new TypedService(property.PropertyType);
            var instanceTypeParameter = new NamedParameter(InstanceTypeNamedParameter, instanceType);
            if (context.TryResolveService(propertyService, new Parameter[] { instanceTypeParameter }, out var propertyValue))
            {
                var setter = ReflectionCache.Shared.Internal.AutowiringPropertySetters.GetOrAdd(property, MakeFastPropertySetter);
                setter(instance, propertyValue);
            }
        }
    }

    private static IEnumerable<PropertyInfo> GetInjectableProperties(Type instanceType)
    {
        foreach (var property in instanceType.GetRuntimeProperties())
        {
            if (!property.CanWrite)
            {
                continue;
            }

            // SetMethod will be non-null if CanWrite is true.
            // Don't want to inject onto static properties.
            if (property.SetMethod!.IsStatic)
            {
                continue;
            }

            var propertyType = property.PropertyType;

            if (propertyType.IsValueType && !propertyType.IsEnum)
            {
                continue;
            }

            // GetElementType will be non-null if IsArray is true.
            if (propertyType.IsArray && propertyType.GetElementType()!.IsValueType)
            {
                continue;
            }

            if (propertyType.IsGenericEnumerableInterfaceType() && propertyType.GenericTypeArguments[0].IsValueType)
            {
                continue;
            }

            if (property.GetIndexParameters().Length != 0)
            {
                continue;
            }

            yield return property;
        }
    }

    private static Action<object, object?> MakeFastPropertySetter(PropertyInfo propertyInfo)
    {
        // SetMethod will be non-null if we're trying to make a setter for it.
        var setMethod = propertyInfo.SetMethod!;

        // DeclaringType will always be set for properties.
        var typeInput = setMethod.DeclaringType!;
        var parameters = setMethod.GetParameters();
        var parameterType = parameters[0].ParameterType;

        // Create a delegate TDeclaringType -> { TDeclaringType.Property = TValue; }
        var propertySetterAsAction = setMethod.CreateDelegate(typeof(Action<,>).MakeGenericType(typeInput, parameterType));
        var callPropertySetterClosedGenericMethod = CallPropertySetterOpenGenericMethod.MakeGenericMethod(typeInput, parameterType);
        var callPropertySetterDelegate = callPropertySetterClosedGenericMethod.CreateDelegate<Action<object, object?>>(propertySetterAsAction);

        return callPropertySetterDelegate;
    }

    private static void CallPropertySetter<TDeclaringType, TValue>(
        Action<TDeclaringType, TValue> setter, object target, object value) =>
            setter((TDeclaringType)target, (TValue)value);
}
