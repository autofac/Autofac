// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Autofac.Core;
using Autofac.Util;

namespace Autofac.Features.GeneratedFactories;

/// <summary>
/// Generates context-bound closures that represent factories from
/// a set of heuristics based on delegate type signatures.
/// </summary>
public class FactoryGenerator
{
    /// <summary>
    /// Cached compiled generators for the <see cref="FactoryGenerator(Type, Service, ParameterMapping)"/> overload
    /// (resolves via <c>ResolveService</c>). Keyed on <c>(delegateType, effectiveParameterMapping)</c>.
    /// Exposed internally for test observability only.
    /// </summary>
    internal static readonly ConcurrentDictionary<(Type, ParameterMapping), Func<Service, IComponentContext, IEnumerable<Parameter>, Delegate>>
        ServiceOnlyGeneratorCache = new();

    /// <summary>
    /// Cached compiled generators for the <see cref="FactoryGenerator(Type, Service, ServiceRegistration, ParameterMapping)"/> overload
    /// (resolves via <see cref="IComponentContext.ResolveComponent"/>). Keyed on <c>(delegateType, effectiveParameterMapping)</c>.
    /// Exposed internally for test observability only.
    /// </summary>
    internal static readonly ConcurrentDictionary<(Type, ParameterMapping), Func<Service, ServiceRegistration, IComponentContext, IEnumerable<Parameter>, Delegate>>
        ServiceRegistrationGeneratorCache = new();

    // The explicit '!' default is ok because the code is never executed, it's just used by
    // the expression tree.
    private static readonly ConstructorInfo _requestConstructor
        = ReflectionExtensions.GetConstructor(() => new ResolveRequest(default!, default!, default!, default));

    private readonly Func<IComponentContext, IEnumerable<Parameter>, Delegate> _generator;

    /// <summary>
    /// Initializes a new instance of the <see cref="FactoryGenerator"/> class.
    /// </summary>
    /// <param name="service">The service that will be activated in
    /// order to create the products of the factory.</param>
    /// <param name="delegateType">The delegate to provide as a factory.</param>
    /// <param name="parameterMapping">The parameter mapping mode to use.</param>
    public FactoryGenerator(Type delegateType, Service service, ParameterMapping parameterMapping)
    {
        if (service == null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        Enforce.ArgumentTypeIsFunction(delegateType);

        var pm = GetParameterMapping(delegateType, parameterMapping);

        // Retrieve the cached compiled generator for this delegate type and parameter mapping,
        // or compile it once on first use. The cached delegate is parameterised on 'service'
        // (passed at invocation time) so it contains no instance-specific captures and is safe
        // to share across all FactoryGenerator instances with the same structural signature.
        var compiledGenerator = ServiceOnlyGeneratorCache.GetOrAdd(
            (delegateType, pm),
            static key => CreateServiceOnlyGenerator(key.Item1, key.Item2));

        // Close over the specific service so _generator matches the expected signature.
        _generator = (context, parameters) => compiledGenerator(service, context, parameters);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FactoryGenerator"/> class.
    /// </summary>
    /// <param name="service">The service that will be activated in
    /// order to create the products of the factory.</param>
    /// <param name="productRegistration">The component that will be activated in
    /// order to create the products of the factory.</param>
    /// <param name="delegateType">The delegate to provide as a factory.</param>
    /// <param name="parameterMapping">The parameter mapping mode to use.</param>
    public FactoryGenerator(Type delegateType, Service service, ServiceRegistration productRegistration, ParameterMapping parameterMapping)
    {
        Enforce.ArgumentTypeIsFunction(delegateType);

        var pm = GetParameterMapping(delegateType, parameterMapping);

        // Retrieve the cached compiled generator for this delegate type and parameter mapping,
        // or compile it once on first use. The cached delegate is parameterised on both 'service'
        // and 'productRegistration' (passed at invocation time), so it contains no instance-specific
        // captures and is safe to share across all FactoryGenerator instances for the same delegate type.
        var compiledGenerator = ServiceRegistrationGeneratorCache.GetOrAdd(
            (delegateType, pm),
            static key => CreateServiceRegistrationGenerator(key.Item1, key.Item2));

        // Close over the specific service and product registration so _generator matches the
        // expected signature. These values are instance-specific but are not baked into the
        // compiled expression tree — they are passed as arguments at each invocation.
        _generator = (context, parameters) => compiledGenerator(service, productRegistration, context, parameters);
    }

    /// <summary>
    /// Generates a factory delegate that closes over the provided context.
    /// </summary>
    /// <param name="context">The context in which the factory will be used.</param>
    /// <param name="parameters">Parameters provided to the resolve call for the factory itself.</param>
    /// <returns>A factory delegate that will work within the context.</returns>
    public Delegate GenerateFactory(IComponentContext context, IEnumerable<Parameter> parameters)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (parameters == null)
        {
            throw new ArgumentNullException(nameof(parameters));
        }

        return _generator(context.Resolve<ILifetimeScope>(), parameters);
    }

    /// <summary>
    /// Generates a factory delegate that closes over the provided context.
    /// </summary>
    /// <typeparam name="TDelegate">The type of the delegate to generate.</typeparam>
    /// <param name="context">The context in which the factory will be used.</param>
    /// <param name="parameters">Parameters provided to the resolve call for the factory itself.</param>
    /// <returns>A factory delegate that will work within the context.</returns>
    public TDelegate GenerateFactory<TDelegate>(IComponentContext context, IEnumerable<Parameter> parameters)
        where TDelegate : class
    {
        return (TDelegate)(object)GenerateFactory(context, parameters);
    }

    private static ParameterMapping GetParameterMapping(Type delegateType, ParameterMapping configuredParameterMapping)
    {
        if (configuredParameterMapping == ParameterMapping.Adaptive)
        {
            return DelegateTypeIsFunc(delegateType) ? ParameterMapping.ByType : ParameterMapping.ByName;
        }

        return configuredParameterMapping;
    }

    private static bool DelegateTypeIsFunc(Type delegateType)
    {
        return delegateType.Name.StartsWith("Func`", StringComparison.Ordinal);
    }

    /// <summary>
    /// Creates a compiled generator for the overload that resolves via <c>ResolveService</c>.
    /// The returned delegate accepts <paramref name="delegateType"/>-specific parameters at runtime and contains
    /// no instance-specific compile-time captures, making it safe to cache and share across all
    /// <see cref="FactoryGenerator"/> instances with the same (<paramref name="delegateType"/>, <paramref name="pm"/>) pair.
    /// </summary>
    private static Func<Service, IComponentContext, IEnumerable<Parameter>, Delegate> CreateServiceOnlyGenerator(Type delegateType, ParameterMapping pm)
    {
        // Outer parameters: (Service svc, IComponentContext c, IEnumerable<Parameter> p)
        var serviceParam = Expression.Parameter(typeof(Service), "svc");
        var activatorContextParam = Expression.Parameter(typeof(IComponentContext), "c");
        var activatorParamsParam = Expression.Parameter(typeof(IEnumerable<Parameter>), "p");

        var invoke = delegateType.GetDeclaredMethod("Invoke");

        // [dps]*
        var creatorParams = invoke
            .GetParameters()
            .Select(pi => Expression.Parameter(pi.ParameterType, pi.Name))
            .ToList();

        Expression? resolveCast = null;
        if (DelegateTypeIsFunc(delegateType) && pm == ParameterMapping.ByType)
        {
            // Issue #269: duplicate input parameter types are not allowed for ByType mapping.
            var arguments = delegateType.GenericTypeArguments;
            var returnType = arguments[arguments.Length - 1];
            Array.Resize(ref arguments, arguments.Length - 1);
            if (arguments.Distinct().Count() != arguments.Length)
            {
                object[] argumentsArray = arguments.ToArray();
                var message = string.Format(CultureInfo.CurrentCulture, GeneratedFactoryRegistrationSourceResources.DuplicateTypesInTypeMappedFuncParameterList, returnType.AssemblyQualifiedName, string.Join(", ", argumentsArray));
                resolveCast = Expression.Throw(Expression.Constant(new DependencyResolutionException(message)), invoke.ReturnType);
            }
        }

        if (resolveCast == null)
        {
            var resolveParameterArray = MapParameters(creatorParams, pm);

            // c.ResolveService(svc, [new Parameter(...)]*) — 'svc' is supplied as a runtime parameter.
            var resolveParams = new Expression[]
            {
                activatorContextParam,
                serviceParam,
                Expression.NewArrayInit(typeof(Parameter), resolveParameterArray),
            };

            var resolveCall = Expression.Call(
                ReflectionExtensions.GetMethod<IComponentContext>(cc => cc.ResolveService(default!, default!)),
                resolveParams);

            resolveCast = Expression.Convert(resolveCall, invoke.ReturnType);
        }

        // ([dps]*) => (drt)c.ResolveService(svc, [...])
        var creator = Expression.Lambda(delegateType, resolveCast, creatorParams);

        // (svc, c, p) => ([dps]*) => ...
        var activator = Expression.Lambda<Func<Service, IComponentContext, IEnumerable<Parameter>, Delegate>>(
            creator,
            serviceParam,
            activatorContextParam,
            activatorParamsParam);

        return activator.Compile();
    }

    /// <summary>
    /// Creates a compiled generator for the overload that resolves via <see cref="IComponentContext.ResolveComponent"/>.
    /// The returned delegate accepts <paramref name="delegateType"/>-specific parameters at runtime and contains
    /// no instance-specific compile-time captures, making it safe to cache and share across all
    /// <see cref="FactoryGenerator"/> instances with the same (<paramref name="delegateType"/>, <paramref name="pm"/>) pair.
    /// </summary>
    private static Func<Service, ServiceRegistration, IComponentContext, IEnumerable<Parameter>, Delegate> CreateServiceRegistrationGenerator(Type delegateType, ParameterMapping pm)
    {
        // Outer parameters: (Service svc, ServiceRegistration sr, IComponentContext c, IEnumerable<Parameter> p)
        var serviceParam = Expression.Parameter(typeof(Service), "svc");
        var serviceRegistrationParam = Expression.Parameter(typeof(ServiceRegistration), "sr");
        var activatorContextParam = Expression.Parameter(typeof(IComponentContext), "c");
        var activatorParamsParam = Expression.Parameter(typeof(IEnumerable<Parameter>), "p");

        var invoke = delegateType.GetDeclaredMethod("Invoke");

        // [dps]*
        var creatorParams = invoke
            .GetParameters()
            .Select(pi => Expression.Parameter(pi.ParameterType, pi.Name))
            .ToList();

        Expression? resolveCast = null;
        if (DelegateTypeIsFunc(delegateType) && pm == ParameterMapping.ByType)
        {
            // Issue #269: duplicate input parameter types are not allowed for ByType mapping.
            var arguments = delegateType.GenericTypeArguments;
            var returnType = arguments[arguments.Length - 1];
            Array.Resize(ref arguments, arguments.Length - 1);
            if (arguments.Distinct().Count() != arguments.Length)
            {
                object[] argumentsArray = arguments.ToArray();
                var message = string.Format(CultureInfo.CurrentCulture, GeneratedFactoryRegistrationSourceResources.DuplicateTypesInTypeMappedFuncParameterList, returnType.AssemblyQualifiedName, string.Join(", ", argumentsArray));
                resolveCast = Expression.Throw(Expression.Constant(new DependencyResolutionException(message)), invoke.ReturnType);
            }
        }

        if (resolveCast == null)
        {
            var resolveParameterArray = MapParameters(creatorParams, pm);

            // new ResolveRequest(svc, sr, [...], null) — 'svc' and 'sr' are runtime parameters.
            var newExpression = Expression.New(
                _requestConstructor,
                serviceParam,
                serviceRegistrationParam,
                Expression.NewArrayInit(typeof(Parameter), resolveParameterArray),
                Expression.Constant(null, typeof(IComponentRegistration)));

            // c.ResolveComponent(new ResolveRequest(...))
            var resolveCall = Expression.Call(
                activatorContextParam,
                ReflectionExtensions.GetMethod<IComponentContext>(cc => cc.ResolveComponent(
                    new ResolveRequest(default!, default, default(Parameter[])!, default))),
                newExpression);

            resolveCast = Expression.Convert(resolveCall, invoke.ReturnType);
        }

        // ([dps]*) => (drt)c.ResolveComponent(new ResolveRequest(svc, sr, [...]))
        var creator = Expression.Lambda(delegateType, resolveCast, creatorParams);

        // (svc, sr, c, p) => ([dps]*) => ...
        var activator = Expression.Lambda<Func<Service, ServiceRegistration, IComponentContext, IEnumerable<Parameter>, Delegate>>(
            creator,
            serviceParam,
            serviceRegistrationParam,
            activatorContextParam,
            activatorParamsParam);

        return activator.Compile();
    }

    private static Expression[] MapParameters(IEnumerable<ParameterExpression> creatorParams, ParameterMapping pm)
    {
        return pm switch
        {
            ParameterMapping.ByType => creatorParams
                                        .Select(p => Expression.New(typeof(TypedParameter).GetMatchingConstructor(new[] { typeof(Type), typeof(object) })!, Expression.Constant(p.Type, typeof(Type)), Expression.Convert(p, typeof(object))))
                                        .ToArray(),
            ParameterMapping.ByPosition => creatorParams
                                             .Select((p, i) => Expression.New(typeof(PositionalParameter).GetMatchingConstructor(new[] { typeof(int), typeof(object) })!, Expression.Constant(i, typeof(int)), Expression.Convert(p, typeof(object))))
                                             .ToArray(),
            _ => creatorParams.Select(p => Expression.New(typeof(NamedParameter).GetMatchingConstructor(new[] { typeof(string), typeof(object) })!, Expression.Constant(p.Name, typeof(string)), Expression.Convert(p, typeof(object))))
                              .ToArray(),
        };
    }
}
