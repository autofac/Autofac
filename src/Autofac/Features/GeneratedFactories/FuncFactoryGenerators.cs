// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#if !NETSTANDARD2_0
using System.Reflection;
using System.Runtime.CompilerServices;
#endif
using Autofac.Core;

namespace Autofac.Features.GeneratedFactories;

/// <summary>
/// Creates generators for type-mapped <see cref="Func{TResult}"/> factories from precompiled
/// generic methods, so the first resolution of each signature does not compile an expression tree.
/// </summary>
/// <remarks>
/// Only <c>System.Func</c> delegates with up to four distinct argument types are handled, and only
/// where code is compiled rather than interpreted. Everything else gets <see langword="null"/> and
/// uses the compiled expression path in <see cref="FactoryGenerator"/>. The generated factories pass
/// the same parameters as that path: one <see cref="TypedParameter"/> per argument, or
/// <see cref="ResolveRequest.NoParameters"/> when there are none.
/// </remarks>
internal static class FuncFactoryGenerators
{
    private const string DynamicCodeWarning = "Generated factories construct generic methods over the factory's argument types at runtime, which is not compatible with native AOT.";

#if !NETSTANDARD2_0
    private const int MaxArguments = 4;

    private static readonly Type[] _funcDefinitions =
    {
        typeof(Func<>),
        typeof(Func<,>),
        typeof(Func<,,>),
        typeof(Func<,,,>),
        typeof(Func<,,,,>),
    };

    // The definitions come from delegates rather than lookups by name, so trimming and AOT analysis
    // see exactly which methods are used.
    private static readonly MethodInfo[] _serviceGenerators =
    {
        Definition(ForService0<object>),
        Definition(ForService1<object, object>),
        Definition(ForService2<object, object, object>),
        Definition(ForService3<object, object, object, object>),
        Definition(ForService4<object, object, object, object, object>),
    };

    private static readonly MethodInfo[] _registrationGenerators =
    {
        Definition(ForRegistration0<object>),
        Definition(ForRegistration1<object, object>),
        Definition(ForRegistration2<object, object, object>),
        Definition(ForRegistration3<object, object, object, object>),
        Definition(ForRegistration4<object, object, object, object, object>),
    };
#endif

    /// <summary>
    /// Gets a generator for a factory that resolves through <c>ResolveService</c>, if the delegate type is supported.
    /// </summary>
    /// <param name="delegateType">The factory delegate type.</param>
    /// <param name="pm">The effective parameter mapping.</param>
    /// <returns>The generator, or <see langword="null"/> to use the compiled expression path.</returns>
    [RequiresDynamicCode(DynamicCodeWarning)]
    public static Func<Service, IComponentContext, Delegate>? TryCreateServiceGenerator(Type delegateType, ParameterMapping pm)
#if NETSTANDARD2_0
        => null;
#else
        => TryCreate<Func<Service, IComponentContext, Delegate>>(_serviceGenerators, delegateType, pm);
#endif

    /// <summary>
    /// Gets a generator for a factory that resolves through <see cref="IComponentContext.ResolveComponent"/>, if the delegate type is supported.
    /// </summary>
    /// <param name="delegateType">The factory delegate type.</param>
    /// <param name="pm">The effective parameter mapping.</param>
    /// <returns>The generator, or <see langword="null"/> to use the compiled expression path.</returns>
    [RequiresDynamicCode(DynamicCodeWarning)]
    public static Func<ResolveRequestTemplate, IComponentContext, Delegate>? TryCreateRegistrationGenerator(Type delegateType, ParameterMapping pm)
#if NETSTANDARD2_0
        => null;
#else
        => TryCreate<Func<ResolveRequestTemplate, IComponentContext, Delegate>>(_registrationGenerators, delegateType, pm);
#endif

#if !NETSTANDARD2_0
    [RequiresDynamicCode(DynamicCodeWarning)]
    [UnconditionalSuppressMessage("Trimming", "IL2060", Justification = "The generic generator methods place no DynamicallyAccessedMembers requirements on their type parameters.")]
    private static TGenerator? TryCreate<TGenerator>(MethodInfo[] generators, Type delegateType, ParameterMapping pm)
        where TGenerator : Delegate
    {
        // Where code is interpreted rather than compiled (Mono full AOT, for example), compiling the
        // expression is cheap and closing generic methods over value types may not be possible.
        if (!RuntimeFeature.IsDynamicCodeCompiled || pm != ParameterMapping.ByType || !delegateType.IsGenericType)
        {
            return null;
        }

        var arguments = delegateType.GenericTypeArguments;
        var argumentCount = arguments.Length - 1;
        if (argumentCount > MaxArguments || delegateType.GetGenericTypeDefinition() != _funcDefinitions[argumentCount])
        {
            return null;
        }

        // Ref struct arguments cannot close these generic methods. Duplicate argument types are
        // left to the compiled expression path, which reports them when the factory is invoked (#269).
        if (arguments.Any(argument => argument.IsByRefLike) || arguments.Take(argumentCount).Distinct().Count() != argumentCount)
        {
            return null;
        }

        return (TGenerator)generators[argumentCount].MakeGenericMethod(arguments).CreateDelegate(typeof(TGenerator));
    }

    private static MethodInfo Definition(Func<Service, IComponentContext, Delegate> generator)
        => generator.Method.GetGenericMethodDefinition();

    private static MethodInfo Definition(Func<ResolveRequestTemplate, IComponentContext, Delegate> generator)
        => generator.Method.GetGenericMethodDefinition();

    private static Delegate ForService0<TResult>(Service service, IComponentContext context)
        => new Func<TResult>(() => (TResult)context.ResolveService(service, ResolveRequest.NoParameters));

    private static Delegate ForService1<T1, TResult>(Service service, IComponentContext context)
        => new Func<T1, TResult>(arg1 => (TResult)context.ResolveService(
            service,
            new TypedParameter(typeof(T1), arg1)));

    private static Delegate ForService2<T1, T2, TResult>(Service service, IComponentContext context)
        => new Func<T1, T2, TResult>((arg1, arg2) => (TResult)context.ResolveService(
            service,
            new TypedParameter(typeof(T1), arg1),
            new TypedParameter(typeof(T2), arg2)));

    private static Delegate ForService3<T1, T2, T3, TResult>(Service service, IComponentContext context)
        => new Func<T1, T2, T3, TResult>((arg1, arg2, arg3) => (TResult)context.ResolveService(
            service,
            new TypedParameter(typeof(T1), arg1),
            new TypedParameter(typeof(T2), arg2),
            new TypedParameter(typeof(T3), arg3)));

    private static Delegate ForService4<T1, T2, T3, T4, TResult>(Service service, IComponentContext context)
        => new Func<T1, T2, T3, T4, TResult>((arg1, arg2, arg3, arg4) => (TResult)context.ResolveService(
            service,
            new TypedParameter(typeof(T1), arg1),
            new TypedParameter(typeof(T2), arg2),
            new TypedParameter(typeof(T3), arg3),
            new TypedParameter(typeof(T4), arg4)));

    private static Delegate ForRegistration0<TResult>(ResolveRequestTemplate template, IComponentContext context)
        => new Func<TResult>(() => (TResult)context.ResolveComponent(template.CreateRequest(ResolveRequest.NoParameters)));

    private static Delegate ForRegistration1<T1, TResult>(ResolveRequestTemplate template, IComponentContext context)
        => new Func<T1, TResult>(arg1 => (TResult)context.ResolveComponent(template.CreateRequest(new Parameter[]
        {
            new TypedParameter(typeof(T1), arg1),
        })));

    private static Delegate ForRegistration2<T1, T2, TResult>(ResolveRequestTemplate template, IComponentContext context)
        => new Func<T1, T2, TResult>((arg1, arg2) => (TResult)context.ResolveComponent(template.CreateRequest(new Parameter[]
        {
            new TypedParameter(typeof(T1), arg1),
            new TypedParameter(typeof(T2), arg2),
        })));

    private static Delegate ForRegistration3<T1, T2, T3, TResult>(ResolveRequestTemplate template, IComponentContext context)
        => new Func<T1, T2, T3, TResult>((arg1, arg2, arg3) => (TResult)context.ResolveComponent(template.CreateRequest(new Parameter[]
        {
            new TypedParameter(typeof(T1), arg1),
            new TypedParameter(typeof(T2), arg2),
            new TypedParameter(typeof(T3), arg3),
        })));

    private static Delegate ForRegistration4<T1, T2, T3, T4, TResult>(ResolveRequestTemplate template, IComponentContext context)
        => new Func<T1, T2, T3, T4, TResult>((arg1, arg2, arg3, arg4) => (TResult)context.ResolveComponent(template.CreateRequest(new Parameter[]
        {
            new TypedParameter(typeof(T1), arg1),
            new TypedParameter(typeof(T2), arg2),
            new TypedParameter(typeof(T3), arg3),
            new TypedParameter(typeof(T4), arg4),
        })));
#endif
}
