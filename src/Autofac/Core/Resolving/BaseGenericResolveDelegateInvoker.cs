// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using System.Runtime.CompilerServices;

namespace Autofac.Core.Resolving;

/// <summary>
/// Provides a base implementation for the dynamically generated DelegateInvoker classes.
/// </summary>
/// <remarks>
/// See Autofac.CodeGen.DelegateRegisterGenerator.
/// </remarks>
internal abstract class BaseGenericResolveDelegateInvoker
{
    private ParameterInfo[]? _methodParameters;

    /// <summary>
    /// Method implemented by the derived generated class to get the <see cref="ParameterInfo"/> array for the owned delegate.
    /// </summary>
    protected abstract ParameterInfo[] GetDelegateParameters();

    /// <summary>
    /// Resolve the specified type parameter either from the currently-in-scope
    /// resolve parameters, or as a regular dependency.
    /// </summary>
    /// <typeparam name="T">The type to resolve.</typeparam>
    /// <param name="context">The context from which to resolve.</param>
    /// <param name="parameters">The set of Autofac resolve parameters.</param>
    /// <param name="parameterInfoPosition">The position of the parameter in the
    /// delegate's parameter info set.</param>
    protected T? ResolveWithParametersOrRegistration<T>(IComponentContext context, IEnumerable<Parameter> parameters, int parameterInfoPosition)
        where T : notnull
    {
        var parameterInfo = (_methodParameters ??= GetDelegateParameters())[parameterInfoPosition];

        foreach (var parameter in parameters)
        {
            if (parameter.CanSupplyValue(parameterInfo, context, out var valueProvider))
            {
                var value = valueProvider();

                return (T?)value;
            }
        }

        return context.Resolve<T>();
    }

    /// <summary>
    /// Checks whether there are any parameters in the set of parameters.
    /// </summary>
    /// <param name="parameters">The parameters.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static bool AnyParameters(IEnumerable<Parameter> parameters)
    {
        // The by-far most common way you'll end up with no parameters is by
        // invoking a Resolve() function call that doesn't accept parameters, so
        // the readonly NoParameters shared value is used.
        // A ReferenceEquals comparison here handles that neatly, and is
        // significantly faster in benchmarks than doing the Any() call in every
        // case.
        if (ReferenceEquals(parameters, ResolveRequest.NoParameters))
        {
            return false;
        }

        // Might be some parameters, so use Any to check for parameters.
        // For a List- or Array-backed call, this is pretty quick.
        return parameters.Any();
    }
}
