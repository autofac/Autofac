// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
#if NETSTANDARD2_0
using System.Runtime.InteropServices;
#else
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
/// where code is JIT-compiled. Everything else gets <see langword="null"/> and uses the compiled
/// expression path in <see cref="FactoryGenerator"/>. The generated factories pass the same
/// parameters as that path: one <see cref="TypedParameter"/> per argument, or
/// <see cref="ResolveRequest.NoParameters"/> when there are none.
/// </remarks>
internal static class FuncFactoryGenerators
{
    private const string DynamicCodeWarning = "Generated factories construct generic methods over the factory's argument types at runtime, which is not compatible with native AOT.";

    private const int MaxArguments = 4;

    // This class deliberately has no static fields: a type initializer would run its reflection
    // setup on runtimes that never use the precompiled generators.
    private static bool PrecompiledGeneratorsSupported =>
#if NETSTANDARD2_0
        // RuntimeFeature is missing here. .NET Framework always JIT-compiles, but older Unity and
        // Xamarin.iOS AOT can load netstandard2.0 too, so only .NET Framework is trusted.
        RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework", StringComparison.Ordinal);
#else
        // Where code is interpreted (Mono full AOT, Native AOT), compiling the expression is cheap
        // and closing generic methods over value types may not be possible.
        RuntimeFeature.IsDynamicCodeCompiled;
#endif

    /// <summary>
    /// Gets a generator for a factory that resolves through <c>ResolveService</c>, if the delegate type is supported.
    /// </summary>
    /// <param name="delegateType">The factory delegate type.</param>
    /// <param name="pm">The effective parameter mapping.</param>
    /// <returns>The generator, or <see langword="null"/> to use the compiled expression path.</returns>
    [RequiresDynamicCode(DynamicCodeWarning)]
    public static Func<Service, IComponentContext, Delegate>? TryCreateServiceGenerator(Type delegateType, ParameterMapping pm)
        => TryCreate<Func<Service, IComponentContext, Delegate>>(delegateType, pm, ServiceGenerator);

    /// <summary>
    /// Gets a generator for a factory that resolves through <see cref="IComponentContext.ResolveComponent"/>, if the delegate type is supported.
    /// </summary>
    /// <param name="delegateType">The factory delegate type.</param>
    /// <param name="pm">The effective parameter mapping.</param>
    /// <returns>The generator, or <see langword="null"/> to use the compiled expression path.</returns>
    [RequiresDynamicCode(DynamicCodeWarning)]
    public static Func<ResolveRequestTemplate, IComponentContext, Delegate>? TryCreateRegistrationGenerator(Type delegateType, ParameterMapping pm)
        => TryCreate<Func<ResolveRequestTemplate, IComponentContext, Delegate>>(delegateType, pm, RegistrationGenerator);

    [RequiresDynamicCode(DynamicCodeWarning)]
    private static TGenerator? TryCreate<TGenerator>(Type delegateType, ParameterMapping pm, Func<int, MethodInfo> definition)
        where TGenerator : Delegate
    {
        var arguments = PrecompiledGeneratorsSupported ? SupportedArguments(delegateType, pm) : null;
        return arguments is null ? null : Create<TGenerator>(definition(arguments.Length - 1), arguments);
    }

    private static Type[]? SupportedArguments(Type delegateType, ParameterMapping pm)
    {
        if (pm != ParameterMapping.ByType || !delegateType.IsGenericType)
        {
            return null;
        }

        var arguments = delegateType.GenericTypeArguments;
        var argumentCount = arguments.Length - 1;
        if (argumentCount > MaxArguments || delegateType.GetGenericTypeDefinition() != FuncDefinition(argumentCount))
        {
            return null;
        }

        // Duplicate argument types are left to the compiled expression path, which reports them
        // when the factory is invoked (#269).
        if (arguments.Take(argumentCount).Distinct().Count() != argumentCount)
        {
            return null;
        }

#if !NETSTANDARD2_0
        // Ref struct arguments cannot close these generic methods. .NET Framework has no
        // ref struct Func arguments, so netstandard2.0 needs no check.
        if (arguments.Any(argument => argument.IsByRefLike))
        {
            return null;
        }
#endif

        return arguments;
    }

    [RequiresDynamicCode(DynamicCodeWarning)]
    [UnconditionalSuppressMessage("Trimming", "IL2060", Justification = "The generic generator methods place no DynamicallyAccessedMembers requirements on their type parameters.")]
    private static TGenerator Create<TGenerator>(MethodInfo definition, Type[] arguments)
        where TGenerator : Delegate
        => (TGenerator)definition.MakeGenericMethod(arguments).CreateDelegate(typeof(TGenerator));

    private static Type FuncDefinition(int argumentCount) => argumentCount switch
    {
        0 => typeof(Func<>),
        1 => typeof(Func<,>),
        2 => typeof(Func<,,>),
        3 => typeof(Func<,,,>),
        _ => typeof(Func<,,,,>),
    };

    // The definitions come from delegates rather than lookups by name, so trimming and AOT analysis
    // see exactly which methods are used.
    private static MethodInfo ServiceGenerator(int argumentCount)
    {
        Func<Service, IComponentContext, Delegate> generator = argumentCount switch
        {
            0 => ForService0<object>,
            1 => ForService1<object, object>,
            2 => ForService2<object, object, object>,
            3 => ForService3<object, object, object, object>,
            _ => ForService4<object, object, object, object, object>,
        };

        return generator.Method.GetGenericMethodDefinition();
    }

    private static MethodInfo RegistrationGenerator(int argumentCount)
    {
        Func<ResolveRequestTemplate, IComponentContext, Delegate> generator = argumentCount switch
        {
            0 => ForRegistration0<object>,
            1 => ForRegistration1<object, object>,
            2 => ForRegistration2<object, object, object>,
            3 => ForRegistration3<object, object, object, object>,
            _ => ForRegistration4<object, object, object, object, object>,
        };

        return generator.Method.GetGenericMethodDefinition();
    }

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
}
