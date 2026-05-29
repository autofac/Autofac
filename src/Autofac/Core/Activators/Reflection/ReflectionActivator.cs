// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Globalization;
using System.Reflection;
using System.Text;
using Autofac.Core.Resolving.Pipeline;
using Autofac.Diagnostics;
using Autofac.Util;

namespace Autofac.Core.Activators.Reflection;

/// <summary>
/// Uses reflection to activate instances of a type.
/// </summary>
public class ReflectionActivator : InstanceActivator, IInstanceActivator
{
    private readonly Type _implementationType;
    private readonly Parameter[] _configuredProperties;
    private readonly Parameter[] _defaultParameters;
    private readonly bool _requiresServiceKeyParameter;

    private ConstructorBinder[]? _constructorBinders;
    private bool _anyRequiredMembers;
    private InjectablePropertyState[]? _defaultFoundPropertySet;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReflectionActivator"/> class.
    /// </summary>
    /// <param name="implementationType">Type to activate.</param>
    /// <param name="constructorFinder">Constructor finder.</param>
    /// <param name="constructorSelector">Constructor selector.</param>
    /// <param name="configuredParameters">Parameters configured explicitly for this instance.</param>
    /// <param name="configuredProperties">Properties configured explicitly for this instance.</param>
    public ReflectionActivator(
        Type implementationType,
        IConstructorFinder constructorFinder,
        IConstructorSelector constructorSelector,
        IEnumerable<Parameter> configuredParameters,
        IEnumerable<Parameter> configuredProperties)
        : base(implementationType)
    {
        if (configuredParameters == null)
        {
            throw new ArgumentNullException(nameof(configuredParameters));
        }

        if (configuredProperties == null)
        {
            throw new ArgumentNullException(nameof(configuredProperties));
        }

        _implementationType = implementationType;
        _requiresServiceKeyParameter = ReflectionCacheSet.Shared.Internal.ServiceKeyUsageByType.GetOrAdd(
            _implementationType,
            static t => UsesServiceKeyAttribute(t));
        ConstructorFinder = constructorFinder ?? throw new ArgumentNullException(nameof(constructorFinder));
        ConstructorSelector = constructorSelector ?? throw new ArgumentNullException(nameof(constructorSelector));
        _configuredProperties = configuredProperties.ToArray();
        _defaultParameters = configuredParameters.Concat(new Parameter[] { new AutowiringParameter(), new DefaultValueParameter() }).ToArray();
    }

    /// <summary>
    /// Gets the constructor finder.
    /// </summary>
    public IConstructorFinder ConstructorFinder
    {
        get;
    }

    /// <summary>
    /// Gets the constructor selector.
    /// </summary>
    public IConstructorSelector ConstructorSelector
    {
        get;
    }

    /// <summary>
    /// Gets a value indicating whether the activation pipeline needs a keyed service parameter for this type.
    /// </summary>
    internal bool RequiresServiceKeyParameter => _requiresServiceKeyParameter;

    /// <inheritdoc/>
    public void ConfigurePipeline(IComponentRegistryServices componentRegistryServices, IResolvePipelineBuilder pipelineBuilder)
    {
        if (componentRegistryServices is null)
        {
            throw new ArgumentNullException(nameof(componentRegistryServices));
        }

        if (pipelineBuilder is null)
        {
            throw new ArgumentNullException(nameof(pipelineBuilder));
        }

        // Precompute required-member and settable-property metadata once at build time.
        _anyRequiredMembers = HasAnyRequiredMembers();
        InitializeInjectablePropertySet();

        // Build constructor binders once; runtime activation reuses them.
        var binders = CreateConstructorBinders();

        if (TryConfigureSingleConstructorActivation(pipelineBuilder, binders))
        {
            return;
        }

        _constructorBinders = binders;

        pipelineBuilder.Use(ToString(), PipelinePhase.Activation, MiddlewareInsertionMode.EndOfPhase, (context, next) =>
        {
            context.Instance = ActivateInstance(context, context.Parameters);

            next(context);
        });
    }

    /// <summary>
    /// Determines if any constructor parameters or settable properties on the
    /// type have a <see cref="ServiceKeyAttribute"/>, which would require
    /// special handling in the activation pipeline.
    /// </summary>
    /// <param name="implementationType">The type to inspect.</param>
    /// <returns><see langword="true"/> if the type uses the <see cref="ServiceKeyAttribute"/>; otherwise, <see langword="false"/>.</returns>
    private static bool UsesServiceKeyAttribute(Type implementationType)
    {
        // Intentionally not picky about _which_ constructor or property has the
        // attribute. If a different constructor is picked via constructor
        // binder/selector, it's fine; we just want to try to shortcut the case
        // where we "may or may not need it." If you mark a property with the
        // attribute but never inject properties, we'll still provide the
        // parameter "just in case" you change your mind at runtime.
        foreach (var constructor in implementationType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            foreach (var parameter in constructor.GetParameters())
            {
                if (ServiceKeyAttributeCache.ParameterHasServiceKey(parameter))
                {
                    return true;
                }
            }
        }

        foreach (var property in implementationType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (property.CanWrite && ServiceKeyAttributeCache.PropertyHasServiceKey(property))
            {
                return true;
            }
        }

        return false;
    }

    private void UseSingleConstructorActivation(IResolvePipelineBuilder pipelineBuilder, ConstructorBinder singleConstructor)
    {
        if (singleConstructor.ParameterCount == 0)
        {
            ConfigureZeroParameterConstructorActivation(pipelineBuilder, singleConstructor);
            return;
        }

        ConfigureBoundSingleConstructorActivation(pipelineBuilder, singleConstructor);
    }

    /// <summary>
    /// Activate an instance in the provided context.
    /// </summary>
    /// <param name="context">Context in which to activate instances.</param>
    /// <param name="parameters">Parameters to the instance.</param>
    /// <returns>The activated instance.</returns>
    /// <remarks>
    /// The context parameter here should probably be ILifetimeScope in order to reveal Disposer,
    /// but will wait until implementing a concrete use case to make the decision.
    /// </remarks>
    private object ActivateInstance(IComponentContext context, IEnumerable<Parameter> parameters)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (parameters == null)
        {
            throw new ArgumentNullException(nameof(parameters));
        }

        CheckNotDisposed();

        var recordMetrics = AutofacMetrics.MetricsEnabled;
        ValueStopwatch instrumentationTimer = default;
        if (recordMetrics)
        {
            instrumentationTimer = ValueStopwatch.StartNew();
        }

        var prioritizedParameters = GetAllParameters(parameters);

        var allBindings = GetAllBindings(_constructorBinders!, context, prioritizedParameters);

        var selectedBinding = ConstructorSelector.SelectConstructorBinding(allBindings, parameters);

        if (!selectedBinding.CanInstantiate)
        {
            throw new InvalidOperationException(string.Format(
                CultureInfo.CurrentCulture,
                ReflectionActivatorResources.ConstructorSelectorCannotSelectAnInvalidBinding,
                ConstructorSelector.GetType().Name));
        }

        var instance = selectedBinding.Instantiate();

        InjectProperties(instance, context, selectedBinding, prioritizedParameters);

        if (recordMetrics)
        {
            AutofacMetrics.RecordReflectionActivation(_implementationType, instrumentationTimer.GetElapsedTime());
        }

        return instance;
    }

    private BoundConstructor[] GetAllBindings(ConstructorBinder[] availableConstructors, IComponentContext context, IEnumerable<Parameter> allParameters)
    {
        var boundConstructors = new BoundConstructor[availableConstructors.Length];
        var validBindings = availableConstructors.Length;

        for (var idx = 0; idx < availableConstructors.Length; idx++)
        {
            var bound = availableConstructors[idx].Bind(allParameters, context);

            boundConstructors[idx] = bound;

            if (!bound.CanInstantiate)
            {
                validBindings--;
            }
        }

        if (validBindings == 0)
        {
            throw new DependencyResolutionException(GetBindingFailureMessage(boundConstructors));
        }

        return boundConstructors;
    }

    private IEnumerable<Parameter> GetAllParameters(IEnumerable<Parameter> parameters)
    {
        // Most often, there will be no `parameters` and/or no `_defaultParameters`; in both of those cases we can avoid allocating.
        // Do a reference compare with the NoParameters constant; faster than an Any() check for the common case.
        if (ReferenceEquals(ResolveRequest.NoParameters, parameters))
        {
            return _defaultParameters;
        }

        if (parameters.Any())
        {
            return EnumerateParameters(parameters);
        }

        return _defaultParameters;
    }

    private IEnumerable<Parameter> EnumerateParameters(IEnumerable<Parameter> parameters)
    {
        foreach (var param in parameters)
        {
            yield return param;
        }

        foreach (var defaultParam in _defaultParameters)
        {
            yield return defaultParam;
        }
    }

    private string GetBindingFailureMessage(BoundConstructor[] constructorBindings)
    {
        var reasons = new StringBuilder();

        foreach (var invalid in constructorBindings.Where(cb => !cb.CanInstantiate))
        {
            reasons.AppendLine();
            reasons.Append(invalid.Description);
        }

        if (ConstructorFinder is DefaultConstructorFinder)
        {
            // Simplify the text for the common default finder case (to make the message easier to understand).
            return string.Format(
                CultureInfo.CurrentCulture,
                ReflectionActivatorResources.NoConstructorsBindableDefaultBinder,
                _implementationType,
                reasons);
        }
        else
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                ReflectionActivatorResources.NoConstructorsBindable,
                ConstructorFinder,
                _implementationType,
                reasons);
        }
    }

    private void InjectProperties(object instance, IComponentContext context, BoundConstructor constructor, IEnumerable<Parameter> allParameters)
    {
        if (!ShouldInjectProperties(constructor))
        {
            return;
        }

        var workingSetOfProperties = (InjectablePropertyState[])_defaultFoundPropertySet!.Clone();
        ApplyConfiguredProperties(instance, context, workingSetOfProperties);

        if (!ShouldValidateRequiredProperties(constructor))
        {
            return;
        }

        ValidateRequiredProperties(instance, context, allParameters, workingSetOfProperties);
    }

    private bool HasAnyRequiredMembers()
    {
        var implementationType = _implementationType;

        return ReflectionCacheSet.Shared.Internal.HasRequiredMemberAttribute.GetOrAdd(
            implementationType,
            static t =>
            {
                // The RequiredMemberAttribute (may)* have Inherit = false on its AttributeUsage options,
                // so walk the tree.
                // (*): see `HasRequiredMemberAttribute` doc for why we dont really know much about the concrete attribute.
                for (var currentType = t; currentType is not null && currentType != typeof(object); currentType = currentType.BaseType)
                {
                    if (currentType.HasRequiredMemberAttribute())
                    {
                        return true;
                    }
                }

                return false;
            });
    }

    private void InitializeInjectablePropertySet()
    {
        if (!_anyRequiredMembers && _configuredProperties.Length == 0)
        {
            return;
        }

        // Get the full set of properties.
        var actualProperties = _implementationType
            .GetRuntimeProperties()
            .Where(pi => pi.CanWrite)
            .ToList();

        _defaultFoundPropertySet = new InjectablePropertyState[actualProperties.Count];

        for (var idx = 0; idx < actualProperties.Count; idx++)
        {
            _defaultFoundPropertySet[idx] = new InjectablePropertyState(new InjectableProperty(actualProperties[idx]));
        }
    }

    private ConstructorBinder[] CreateConstructorBinders()
    {
        // Locate the possible constructors at container build time.
        var availableConstructors = ConstructorFinder.FindConstructors(_implementationType);

        if (availableConstructors.Length == 0)
        {
            throw new NoConstructorsFoundException(_implementationType, ConstructorFinder);
        }

        var binders = new ConstructorBinder[availableConstructors.Length];

        for (var idx = 0; idx < availableConstructors.Length; idx++)
        {
            binders[idx] = new ConstructorBinder(availableConstructors[idx]);
        }

        return binders;
    }

    private bool TryConfigureSingleConstructorActivation(IResolvePipelineBuilder pipelineBuilder, ConstructorBinder[] binders)
    {
        if (binders.Length == 1)
        {
            UseSingleConstructorActivation(pipelineBuilder, binders[0]);
            return true;
        }

        if (ConstructorSelector is not IConstructorSelectorWithEarlyBinding earlyBindingSelector)
        {
            return false;
        }

        var matchedConstructor = earlyBindingSelector.SelectConstructorBinder(binders);
        if (matchedConstructor is null)
        {
            return false;
        }

        UseSingleConstructorActivation(pipelineBuilder, matchedConstructor);
        return true;
    }

    private void ConfigureZeroParameterConstructorActivation(IResolvePipelineBuilder pipelineBuilder, ConstructorBinder singleConstructor)
    {
        var constructorInvoker = singleConstructor.GetConstructorInvoker() ?? throw new NoConstructorsFoundException(_implementationType, ConstructorFinder);

        // If there are no arguments to the constructor, bypass all argument binding and pre-bind the constructor.
        var boundConstructor = BoundConstructor.ForBindSuccess(
            singleConstructor,
            constructorInvoker,
            Array.Empty<Func<object?>>());

        // Fast-path to just create an instance.
        pipelineBuilder.Use(ToString(), PipelinePhase.Activation, MiddlewareInsertionMode.EndOfPhase, (context, next) =>
        {
            CheckNotDisposed();

            var recordMetrics = AutofacMetrics.MetricsEnabled;
            ValueStopwatch instrumentationTimer = default;
            if (recordMetrics)
            {
                instrumentationTimer = ValueStopwatch.StartNew();
            }

            var instance = boundConstructor.Instantiate();

            if (ShouldInjectProperties(boundConstructor))
            {
                var prioritizedParameters = GetAllParameters(context.Parameters);
                InjectProperties(instance, context, boundConstructor, prioritizedParameters);
            }

            context.Instance = instance;

            if (recordMetrics)
            {
                AutofacMetrics.RecordReflectionActivation(_implementationType, instrumentationTimer.GetElapsedTime());
            }

            next(context);
        });
    }

    private void ConfigureBoundSingleConstructorActivation(IResolvePipelineBuilder pipelineBuilder, ConstructorBinder singleConstructor)
    {
        pipelineBuilder.Use(ToString(), PipelinePhase.Activation, MiddlewareInsertionMode.EndOfPhase, (context, next) =>
        {
            CheckNotDisposed();

            var recordMetrics = AutofacMetrics.MetricsEnabled;
            ValueStopwatch instrumentationTimer = default;
            if (recordMetrics)
            {
                instrumentationTimer = ValueStopwatch.StartNew();
            }

            var prioritizedParameters = GetAllParameters(context.Parameters);

            var bound = singleConstructor.Bind(prioritizedParameters, context);

            if (!bound.CanInstantiate)
            {
                throw new DependencyResolutionException(GetBindingFailureMessage(new[] { bound }));
            }

            var instance = bound.Instantiate();

            InjectProperties(instance, context, bound, prioritizedParameters);

            context.Instance = instance;

            if (recordMetrics)
            {
                AutofacMetrics.RecordReflectionActivation(_implementationType, instrumentationTimer.GetElapsedTime());
            }

            next(context);
        });
    }

    private void ApplyConfiguredProperties(object instance, IComponentContext context, InjectablePropertyState[] workingSetOfProperties)
    {
        foreach (var configuredProperty in _configuredProperties)
        {
            for (var propIdx = 0; propIdx < workingSetOfProperties.Length; propIdx++)
            {
                ref var prop = ref workingSetOfProperties[propIdx];

                if (prop.Set)
                {
                    // Skip, already seen.
                    continue;
                }

                if (prop.Property.TrySupplyValue(instance, configuredProperty, context))
                {
                    prop.Set = true;
                    break;
                }
            }
        }
    }

    private bool ShouldValidateRequiredProperties(BoundConstructor constructor)
        => _anyRequiredMembers && !constructor.SetsRequiredMembers;

    private void ValidateRequiredProperties(
        object instance,
        IComponentContext context,
        IEnumerable<Parameter> allParameters,
        InjectablePropertyState[] workingSetOfProperties)
    {
        var failingRequiredProperties = FindUnresolvedRequiredProperties(instance, context, allParameters, workingSetOfProperties);

        if (failingRequiredProperties is not null)
        {
            throw new DependencyResolutionException(BuildRequiredPropertyResolutionMessage(failingRequiredProperties));
        }
    }

    private List<InjectableProperty>? FindUnresolvedRequiredProperties(
        object instance,
        IComponentContext context,
        IEnumerable<Parameter> allParameters,
        InjectablePropertyState[] workingSetOfProperties)
    {
        List<InjectableProperty>? failingRequiredProperties = null;

        for (var propIdx = 0; propIdx < workingSetOfProperties.Length; propIdx++)
        {
            ref var prop = ref workingSetOfProperties[propIdx];

            if (!ShouldAttemptRequiredPropertyPopulation(prop))
            {
                continue;
            }

            if (TryPopulateRequiredProperty(instance, context, allParameters, ref prop))
            {
                continue;
            }

            failingRequiredProperties ??= new();
            failingRequiredProperties.Add(prop.Property);
        }

        return failingRequiredProperties;
    }

    private bool ShouldAttemptRequiredPropertyPopulation(InjectablePropertyState prop)
    {
        // Only unresolved required members participate in this pass.
        return _anyRequiredMembers && prop.Property.IsRequired && !prop.Set;
    }

    private bool TryPopulateRequiredProperty(
        object instance,
        IComponentContext context,
        IEnumerable<Parameter> allParameters,
        ref InjectablePropertyState prop)
    {
        if (!_anyRequiredMembers)
        {
            return false;
        }

        foreach (var parameter in allParameters)
        {
            if (parameter is NamedParameter || parameter is PositionalParameter)
            {
                // Skip Named and Positional parameters, because if someone uses 'value' as a
                // constructor parameter name, it would also match the property, and cause confusion.
                continue;
            }

            if (prop.Property.TrySupplyValue(instance, parameter, context))
            {
                prop.Set = true;
                return true;
            }
        }

        return false;
    }

    private bool ShouldInjectProperties(BoundConstructor constructor)
    {
        if (_defaultFoundPropertySet is null)
        {
            return false;
        }

        return _configuredProperties.Length != 0 || !constructor.SetsRequiredMembers;
    }

    private string BuildRequiredPropertyResolutionMessage(IReadOnlyList<InjectableProperty> failingRequiredProperties)
    {
        var propertyDescriptions = new StringBuilder();

        foreach (var failed in failingRequiredProperties)
        {
            propertyDescriptions.AppendLine();
            propertyDescriptions.Append(failed.Property.Name);
            propertyDescriptions.Append(" (");
            propertyDescriptions.Append(failed.Property.PropertyType.FullName);
            propertyDescriptions.Append(')');
        }

        return string.Format(
            CultureInfo.CurrentCulture,
            ReflectionActivatorResources.RequiredPropertiesCouldNotBeBound,
            _implementationType,
            propertyDescriptions);
    }
}
