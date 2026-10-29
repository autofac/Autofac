// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

using Autofac.Util;

namespace Autofac.Core.Activators.Reflection;

/// <summary>
/// Provides the functionality to bind a single constructor at resolve time.
/// </summary>
public class ConstructorBinder
{
    private static readonly Func<ConstructorInfo, Func<object?[], object>> _factoryBuilder = GetConstructorInvoker;

    private readonly ParameterInfo[] _constructorArgs;
    private readonly Func<object?[], object>? _factory;
    private readonly ParameterInfo? _illegalParameter;

    // The service each argument autowires to, created on first bind rather than on every bind. Not in the
    // constructor: binders for registrations in short-lived child scopes are often never bound.
    private TypedService[]? _argumentServices;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConstructorBinder"/> class.
    /// </summary>
    /// <param name="constructorInfo">The constructor.</param>
    public ConstructorBinder(ConstructorInfo constructorInfo)
    {
        Constructor = constructorInfo ?? throw new ArgumentNullException(nameof(constructorInfo));
        _constructorArgs = constructorInfo.GetParameters();

        SetsRequiredMembers = constructorInfo.HasSetsRequiredMembersAttribute();

        // If any of the parameters are unsafe, do not create an invoker, and store the parameter
        // that broke the rule.
        _illegalParameter = DetectIllegalParameter(_constructorArgs);

        if (_illegalParameter is null)
        {
            var factoryCache = ReflectionCacheSet.Shared.Internal.ConstructorBinderFactory;

            // Build the invoker.
            _factory = factoryCache.GetOrAdd(constructorInfo, _factoryBuilder);
        }
    }

    /// <summary>
    /// Gets the constructor this binder is responsible for binding.
    /// </summary>
    public ConstructorInfo Constructor
    {
        get;
    }

    /// <summary>
    /// Gets a value indicating whether the constructor has the SetsRequiredMembers attribute,
    /// indicating we can skip population of required properties.
    /// </summary>
    public bool SetsRequiredMembers
    {
        get;
    }

    /// <summary>
    /// Gets the set of parameters to bind against.
    /// </summary>
    public IReadOnlyList<ParameterInfo> Parameters => _constructorArgs;

    /// <summary>
    /// Gets the number of parameters.
    /// </summary>
    public int ParameterCount => _constructorArgs.Length;

    /// <summary>
    /// Binds the set of parameters to the constructor. <see cref="BoundConstructor.CanInstantiate"/> indicates success.
    /// </summary>
    /// <param name="availableParameters">The set of all parameters.</param>
    /// <param name="context">The current component context.</param>
    /// <returns>The bind result.</returns>
    public BoundConstructor Bind(IEnumerable<Parameter> availableParameters, IComponentContext context)
    {
        if (availableParameters is null)
        {
            throw new ArgumentNullException(nameof(availableParameters));
        }

        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var constructorArgs = _constructorArgs;
        var constructorArgLength = constructorArgs.Length;

        if (constructorArgLength == 0)
        {
            // No args, auto-bind with an empty value-retriever array to avoid the allocation.
            return BoundConstructor.ForBindSuccess(this, _factory!, Array.Empty<Func<object?>>());
        }

        if (_illegalParameter is not null)
        {
            return BoundConstructor.ForBindFailure(this, _illegalParameter);
        }

        // Iterate an array; foreach over IEnumerable<T> would allocate an enumerator per constructor argument.
        var parameters = availableParameters as Parameter[] ?? availableParameters.ToArray();
        var arguments = new BoundArgument[constructorArgLength];

        // Concurrent first binds may each create the services; any of the results is equivalent.
        var argumentServices = _argumentServices ??= CreateArgumentServices(constructorArgs);

        for (var idx = 0; idx < constructorArgLength; idx++)
        {
            if (!TryBindArgument(parameters, constructorArgs[idx], argumentServices[idx], context, out arguments[idx]))
            {
                return BoundConstructor.ForBindFailure(this, constructorArgs[idx]);
            }
        }

        return new BoundConstructor(this, _factory!, arguments, context);
    }

    /// <summary>
    /// Get the constructor factory delegate.
    /// </summary>
    /// <remarks>Will return null if the constructor contains an invalid parameter.</remarks>
    /// <returns>
    /// A delegate that can be used to invoke the constructor, or <see langword="null"/> if the constructor is invalid.
    /// </returns>
    internal Func<object?[], object>? GetConstructorInvoker()
    {
        return _factory;
    }

    private static TypedService[] CreateArgumentServices(ParameterInfo[] constructorArgs)
    {
        var services = new TypedService[constructorArgs.Length];
        for (var idx = 0; idx < constructorArgs.Length; idx++)
        {
            services[idx] = new TypedService(constructorArgs[idx].ParameterType);
        }

        return services;
    }

    // Not inlined: the JIT would otherwise inline every parameter check into Bind, spilling its loop to the stack.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool TryBindArgument(Parameter[] parameters, ParameterInfo pi, TypedService argumentService, IComponentContext context, out BoundArgument argument)
    {
        foreach (var param in parameters)
        {
            // Bind autowired arguments to their registration directly rather than through a delegate. Only the exact
            // type qualifies, since a subclass may override CanSupplyValue.
            if (param.GetType() == typeof(AutowiringParameter))
            {
                if (AutowiringParameter.TryGetServiceRegistration(pi, context, argumentService, out var service, out var implementation))
                {
                    argument = new BoundArgument(service, implementation);
                    return true;
                }
            }
            else if (param.CanSupplyValue(pi, context, out var valueRetriever))
            {
                argument = new BoundArgument(valueRetriever);
                return true;
            }
        }

        argument = default;
        return false;
    }

    private static Func<object?[], object> GetConstructorInvoker(ConstructorInfo constructorInfo)
    {
        var paramsInfo = constructorInfo.GetParameters();

        var parametersExpression = Expression.Parameter(typeof(object[]), "args");
        var argumentsExpression = new Expression[paramsInfo.Length];

        for (var paramIndex = 0; paramIndex < paramsInfo.Length; paramIndex++)
        {
            var indexExpression = Expression.Constant(paramIndex);
            var parameterType = paramsInfo[paramIndex].ParameterType;

            var parameterIndexExpression = Expression.ArrayIndex(parametersExpression, indexExpression);

            var convertExpression = parameterType.IsPrimitive
                ? Expression.Convert(ConvertPrimitiveType(parameterIndexExpression, parameterType), parameterType)
                : Expression.Convert(parameterIndexExpression, parameterType);

            if (!parameterType.IsValueType)
            {
                argumentsExpression[paramIndex] = convertExpression;
                continue;
            }

            var nullConditionExpression = Expression.Equal(
                parameterIndexExpression, Expression.Constant(null));
            argumentsExpression[paramIndex] = Expression.Condition(
                nullConditionExpression, Expression.Default(parameterType), convertExpression);
        }

        var newExpression = Expression.New(constructorInfo, argumentsExpression);
        var lambdaExpression = Expression.Lambda<Func<object?[], object>>(newExpression, parametersExpression);

        return lambdaExpression.Compile();
    }

    private static MethodCallExpression ConvertPrimitiveType(Expression valueExpression, Type conversionType)
    {
        var changeTypeMethod = typeof(Convert).GetRuntimeMethod(nameof(Convert.ChangeType), new[] { typeof(object), typeof(Type) });

        // changeTypeMethod will always be non-null; Convert.ChangeType definitely exists.
        return Expression.Call(changeTypeMethod!, valueExpression, Expression.Constant(conversionType));
    }

    private static ParameterInfo? DetectIllegalParameter(ParameterInfo[] constructorArgs)
    {
        for (var idx = 0; idx < constructorArgs.Length; idx++)
        {
            var arg = constructorArgs[idx].ParameterType;
            if (arg.IsPointer || arg.IsByRef)
            {
                // Boo.
                return constructorArgs[idx];
            }
        }

        return null;
    }
}
