// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Globalization;
using System.Reflection;

namespace Autofac.Core.Activators.Reflection;

/// <summary>
/// Represents the outcome of a single bind attempt by a <see cref="ConstructorBinder"/>.
/// </summary>
[SuppressMessage(
    "Performance",
    "CA1815:Override equals and operator equals on value types",
    Justification = "Comparison of two BoundConstructor instances has no meaning.")]
public class BoundConstructor
{
    private readonly Func<object?[], object>? _factory;
    private readonly BoundArgument[]? _arguments;
    private readonly IComponentContext? _context;
    private readonly ParameterInfo? _firstNonBindableParameter;

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundConstructor"/> class for a successful bind.
    /// </summary>
    /// <param name="binder">The binder that generated this binding.</param>
    /// <param name="factory">The instance factory.</param>
    /// <param name="valueRetrievers">The set of value-retrieval functions.</param>
    internal BoundConstructor(ConstructorBinder binder, Func<object?[], object> factory, Func<object?>[] valueRetrievers)
        : this(binder, factory, ToArguments(valueRetrievers), context: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundConstructor"/> class for a successful bind.
    /// </summary>
    /// <param name="binder">The binder that generated this binding.</param>
    /// <param name="factory">The instance factory.</param>
    /// <param name="arguments">The sources of the argument values.</param>
    /// <param name="context">The context to resolve autowired arguments from.</param>
    internal BoundConstructor(ConstructorBinder binder, Func<object?[], object> factory, BoundArgument[] arguments, IComponentContext? context)
    {
        CanInstantiate = true;
        Binder = binder;
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _arguments = arguments;
        _context = context;
        _firstNonBindableParameter = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundConstructor"/> class, for an unsuccessful bind.
    /// </summary>
    /// <param name="binder">The binder that generated this binding.</param>
    /// <param name="firstNonBindableParameter">The first parameter that prevented binding.</param>
    internal BoundConstructor(ConstructorBinder binder, ParameterInfo firstNonBindableParameter)
    {
        Binder = binder;
        CanInstantiate = false;
        _firstNonBindableParameter = firstNonBindableParameter ?? throw new ArgumentNullException(nameof(firstNonBindableParameter));
        _factory = null;
        _arguments = null;
        _context = null;
    }

    /// <summary>
    /// Gets the binder that created this binding.
    /// </summary>
    public ConstructorBinder Binder
    {
        get;
    }

    /// <summary>
    /// Gets a value indicating whether the constructor has the SetsRequiredMembers attribute,
    /// indicating we can skip population of required properties.
    /// </summary>
    public bool SetsRequiredMembers => Binder.SetsRequiredMembers;

    /// <summary>
    /// Gets the constructor on the target type. The actual constructor used
    /// might differ, e.g. if using a dynamic proxy.
    /// </summary>
    public ConstructorInfo TargetConstructor => Binder.Constructor;

    /// <summary>
    /// Gets the total number of arguments for the bound constructor.
    /// </summary>
    public int ArgumentCount => Binder.ParameterCount;

    /// <summary>
    /// Gets a value indicating whether the binding is valid.
    /// </summary>
    public bool CanInstantiate
    {
        get;
    }

    /// <summary>
    /// Gets a description of the constructor parameter binding.
    /// </summary>
    public string Description => CanInstantiate
        ? string.Format(CultureInfo.CurrentCulture, BoundConstructorResources.BoundConstructor, TargetConstructor)
        : string.Format(CultureInfo.CurrentCulture, BoundConstructorResources.NonBindableConstructor, TargetConstructor, _firstNonBindableParameter);

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundConstructor"/> class for a successful bind.
    /// </summary>
    /// <param name="binder">The binder that generated this binding.</param>
    /// <param name="factory">The instance factory.</param>
    /// <param name="valueRetrievers">The set of value-retrieval functions.</param>
    /// <returns>A <see cref="BoundConstructor"/> with details about the successful bind.</returns>
    [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Validated in constructor.")]
    public static BoundConstructor ForBindSuccess(ConstructorBinder binder, Func<object?[], object> factory, Func<object?>[] valueRetrievers)
        => new(binder, factory, valueRetrievers);

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundConstructor"/> class, for an unsuccessful bind.
    /// </summary>
    /// <param name="binder">The binder that generated this binding.</param>
    /// <param name="firstNonBindableParameter">The first parameter that prevented binding.</param>
    /// <returns>A <see cref="BoundConstructor"/> with details about the unsuccessful bind.</returns>
    public static BoundConstructor ForBindFailure(ConstructorBinder binder, ParameterInfo firstNonBindableParameter)
        => new(binder, firstNonBindableParameter);

    /// <summary>
    /// Invoke the constructor with the parameter bindings.
    /// </summary>
    /// <returns>The constructed instance.</returns>
    public object Instantiate()
    {
        if (!CanInstantiate)
        {
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, BoundConstructorResources.CannotInstantiate, Description));
        }

        object?[] values;
        var arguments = _arguments!;

        if (arguments.Length != 0)
        {
            values = new object?[arguments.Length];
            for (var i = 0; i < arguments.Length; ++i)
            {
                values[i] = arguments[i].GetValue(_context);
            }
        }
        else
        {
            values = Array.Empty<object?>();
        }

        try
        {
            return _factory!(values);
        }
        catch (TargetInvocationException ex)
        {
            throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, BoundConstructorResources.ExceptionDuringInstantiation, TargetConstructor, TargetConstructor.DeclaringType!.Name), ex.InnerException);
        }
        catch (Exception ex)
        {
            throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, BoundConstructorResources.ExceptionDuringInstantiation, TargetConstructor, TargetConstructor.DeclaringType!.Name), ex);
        }
    }

    /// <summary>Returns a System.String that represents the current System.Object.</summary>
    /// <returns>A System.String that represents the current System.Object.</returns>
    public override string ToString()
    {
        return Description;
    }

    private static BoundArgument[] ToArguments(Func<object?>[] valueRetrievers)
    {
        if (valueRetrievers is null)
        {
            throw new ArgumentNullException(nameof(valueRetrievers));
        }

        if (valueRetrievers.Length == 0)
        {
            return Array.Empty<BoundArgument>();
        }

        var arguments = new BoundArgument[valueRetrievers.Length];
        for (var i = 0; i < valueRetrievers.Length; i++)
        {
            arguments[i] = new BoundArgument(valueRetrievers[i]);
        }

        return arguments;
    }
}
