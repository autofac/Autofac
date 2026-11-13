// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Diagnostics;
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

    private static readonly MethodInfo _callPropertySetterOpenGenericMethod =
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
        var recordMetrics = AutofacMetrics.MetricsEnabled;
        ValueStopwatch instrumentationTimer = default;
        if (recordMetrics)
        {
            instrumentationTimer = ValueStopwatch.StartNew();
        }

        var injectablePropertiesCache = ReflectionCacheSet.Shared.Internal.AutowiringInjectableProperties;

        var instanceType = instance.GetType();
        var injectableProperties = injectablePropertiesCache.GetOrAdd(instanceType, GetInjectableProperties);
        var defaultPropertySelector = propertySelector.GetType() == typeof(DefaultPropertySelector) ? (DefaultPropertySelector)propertySelector : null;
        var injectedProperties = 0;

        for (var index = 0; index < injectableProperties.Length; index++)
        {
            var property = injectableProperties[index];

            if (!IsSelected(property, instance, propertySelector, defaultPropertySelector))
            {
                continue;
            }

            if (TryFindValueProvider(resolveParameters, property.SetterParameter, context, out var valueProvider))
            {
                SetValue(property, instance, valueProvider());
                injectedProperties++;
                continue;
            }

            if (context.TryResolveService(property.Service, property.InstanceTypeParameters, out var propertyValue))
            {
                SetValue(property, instance, propertyValue);
                injectedProperties++;
            }
        }

        if (recordMetrics)
        {
            AutofacMetrics.RecordPropertyInjection(
                instanceType,
                injectableProperties.Length,
                injectedProperties,
                instrumentationTimer.GetElapsedTime());
        }
    }

    // An exact DefaultPropertySelector uses the property checks cached per type. Subclasses may override InjectProperty, so they are asked.
    private static bool IsSelected(AutowiringInjectableProperty property, object instance, IPropertySelector propertySelector, DefaultPropertySelector? defaultPropertySelector)
    {
        if (defaultPropertySelector is null)
        {
            return propertySelector.InjectProperty(property.Property, instance);
        }

        return property.IsSelectedByDefault && defaultPropertySelector.InjectIntoInstance(property.Property, instance);
    }

    private static bool TryFindValueProvider(Parameter[] parameters, ParameterInfo setterParameter, IComponentContext context, [NotNullWhen(returnValue: true)] out Func<object?>? valueProvider)
    {
        foreach (var parameter in parameters)
        {
            // Issue #1275: If a delegate factory has a named parameter 'value' it will try to
            // inject into any property being autowired.
            if (parameter.CanSupplyValue(setterParameter, context, out valueProvider) &&
                !(parameter is NamedParameter n && n.Name.Equals("value", StringComparison.Ordinal)))
            {
                return true;
            }
        }

        valueProvider = null;
        return false;
    }

    private static void SetValue(AutowiringInjectableProperty property, object instance, object? value)
    {
        var setter = property.Setter ??= ReflectionCacheSet.Shared.Internal.AutowiringPropertySetters.GetOrAdd(property.Property, MakeFastPropertySetter);
        setter(instance, value);
    }

    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2067:UnrecognizedReflectionPattern",
        Justification = "instanceType is the runtime type of an already-constructed instance being property-injected. Its public properties are preserved by the activation contract (ActivatorMemberTypes) for reflection-activated types; for externally-provided instances the caller that opted into property injection is responsible for preserving them.")]
    private static AutowiringInjectableProperty[] GetInjectableProperties(Type instanceType)
    {
        var properties = instanceType.GetRuntimeProperties().Where(IsInjectableProperty).ToArray();
        if (properties.Length == 0)
        {
            return Array.Empty<AutowiringInjectableProperty>();
        }

        // One instance type parameter serves every property, and every resolve, of this type.
        var instanceTypeParameters = new Parameter[] { new NamedParameter(InstanceTypeNamedParameter, instanceType) };
        var injectableProperties = new AutowiringInjectableProperty[properties.Length];
        for (var index = 0; index < properties.Length; index++)
        {
            injectableProperties[index] = new AutowiringInjectableProperty(properties[index], instanceTypeParameters);
        }

        return injectableProperties;
    }

    private static bool IsInjectableProperty(PropertyInfo property)
    {
        // We only inject into assignable instance properties.
        if (!property.CanWrite)
        {
            return false;
        }

        // SetMethod will be non-null if CanWrite is true.
        // Don't want to inject onto static properties.
        if (property.SetMethod!.IsStatic)
        {
            return false;
        }

        // Avoid attempting resolution for value-type shapes that Autofac does not
        // meaningfully construct via property injection.
        var propertyType = property.PropertyType;
        if (IsUnsupportedPropertyType(propertyType))
        {
            return false;
        }

        // Indexers require index arguments and are not regular injectable properties.
        return property.GetIndexParameters().Length == 0;
    }

    private static bool IsUnsupportedPropertyType(Type propertyType)
    {
        // Primitive/value-type properties are not autowired (enums are allowed).
        if (propertyType.IsValueType && !propertyType.IsEnum)
        {
            return true;
        }

        // Arrays of value types behave like value containers; skip autowiring.
        // GetElementType will be non-null if IsArray is true.
        if (propertyType.IsArray && propertyType.GetElementType()!.IsValueType)
        {
            return true;
        }

        // Also skip IEnumerable<TValueType> - same rule as arrays above.
        return propertyType.IsGenericEnumerableInterfaceType() && propertyType.GenericTypeArguments[0].IsValueType;
    }

    [SuppressMessage("S125", "S125", Justification = "Commented code explains the code generation output.")]
    [UnconditionalSuppressMessage(
        "AOT",
        "IL3050:RequiresDynamicCode",
        Justification = "Builds a strongly-typed property setter delegate via MakeGenericType/MakeGenericMethod over the property's declaring and value types. Reachable only when property injection is configured for a registration; the consumer that opted into property injection takes on the dynamic-code requirement for value-typed properties.")]
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2060:MakeGenericMethod",
        Justification = "The generic arguments are the property's declaring type and value type, both reachable from the property being injected. Preserving them is the responsibility of the consumer that opted into property injection.")]
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
        var callPropertySetterClosedGenericMethod = _callPropertySetterOpenGenericMethod.MakeGenericMethod(typeInput, parameterType);
        var callPropertySetterDelegate = callPropertySetterClosedGenericMethod.CreateDelegate<Action<object, object?>>(propertySetterAsAction);

        return callPropertySetterDelegate;
    }

    private static void CallPropertySetter<TDeclaringType, TValue>(
        Action<TDeclaringType, TValue> setter, object target, object value)
            => setter((TDeclaringType)target, (TValue)value);
}
