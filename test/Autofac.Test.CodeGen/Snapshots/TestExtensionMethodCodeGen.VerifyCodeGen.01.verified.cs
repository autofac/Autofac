//HintName: Autofac.RegistrationExtensions.g.cs
// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;
using Autofac.Core;

namespace Autofac;

/// <summary>
/// Adds registration syntax to the <see cref="ContainerBuilder"/> type.
/// </summary>
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
public static partial class RegistrationExtensions
{

    /// <summary>
    /// Register a delegate as a component.
    /// </summary>
    /// <typeparam name="TDependency1">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TComponent">The type of the instance.</typeparam>
    /// <param name="builder">Container builder.</param>
    /// <param name="delegate">The delegate to register.</param>
    /// <returns>Registration builder allowing the registration to be configured.</returns>
    public static IRegistrationBuilder<TComponent, SimpleActivatorData, SingleRegistrationStyle>
        Register<TDependency1, TComponent> (
            this ContainerBuilder builder,
            Func<TDependency1, TComponent> @delegate)
        where TDependency1 : notnull
    {
        if (@delegate is null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        var delegateInvoker = new DelegateInvokers.DelegateInvoker1<TDependency1, TComponent>(@delegate);

        return builder.Register(delegateInvoker.ResolveWithDelegate);
    }

    /// <summary>
    /// Register a delegate as a component.
    /// </summary>
    /// <typeparam name="TDependency1">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TComponent">The type of the instance.</typeparam>
    /// <param name="builder">Container builder.</param>
    /// <param name="delegate">The delegate to register.</param>
    /// <returns>Registration builder allowing the registration to be configured.</returns>
    public static IRegistrationBuilder<TComponent, SimpleActivatorData, SingleRegistrationStyle>
        Register<TDependency1, TComponent> (
            this ContainerBuilder builder,
            Func<IComponentContext, TDependency1, TComponent> @delegate)
        where TDependency1 : notnull
    {
        if (@delegate is null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        var delegateInvoker = new DelegateInvokers.DelegateInvoker1WithComponentContext<TDependency1, TComponent>(@delegate);

        return builder.Register(delegateInvoker.ResolveWithDelegate);
    }

    /// <summary>
    /// Register a delegate as a component.
    /// </summary>
    /// <typeparam name="TDependency1">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency2">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TComponent">The type of the instance.</typeparam>
    /// <param name="builder">Container builder.</param>
    /// <param name="delegate">The delegate to register.</param>
    /// <returns>Registration builder allowing the registration to be configured.</returns>
    public static IRegistrationBuilder<TComponent, SimpleActivatorData, SingleRegistrationStyle>
        Register<TDependency1, TDependency2, TComponent> (
            this ContainerBuilder builder,
            Func<TDependency1, TDependency2, TComponent> @delegate)
        where TDependency1 : notnull
        where TDependency2 : notnull
    {
        if (@delegate is null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        var delegateInvoker = new DelegateInvokers.DelegateInvoker2<TDependency1, TDependency2, TComponent>(@delegate);

        return builder.Register(delegateInvoker.ResolveWithDelegate);
    }

    /// <summary>
    /// Register a delegate as a component.
    /// </summary>
    /// <typeparam name="TDependency1">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency2">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TComponent">The type of the instance.</typeparam>
    /// <param name="builder">Container builder.</param>
    /// <param name="delegate">The delegate to register.</param>
    /// <returns>Registration builder allowing the registration to be configured.</returns>
    public static IRegistrationBuilder<TComponent, SimpleActivatorData, SingleRegistrationStyle>
        Register<TDependency1, TDependency2, TComponent> (
            this ContainerBuilder builder,
            Func<IComponentContext, TDependency1, TDependency2, TComponent> @delegate)
        where TDependency1 : notnull
        where TDependency2 : notnull
    {
        if (@delegate is null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        var delegateInvoker = new DelegateInvokers.DelegateInvoker2WithComponentContext<TDependency1, TDependency2, TComponent>(@delegate);

        return builder.Register(delegateInvoker.ResolveWithDelegate);
    }

    /// <summary>
    /// Register a delegate as a component.
    /// </summary>
    /// <typeparam name="TDependency1">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency2">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency3">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TComponent">The type of the instance.</typeparam>
    /// <param name="builder">Container builder.</param>
    /// <param name="delegate">The delegate to register.</param>
    /// <returns>Registration builder allowing the registration to be configured.</returns>
    public static IRegistrationBuilder<TComponent, SimpleActivatorData, SingleRegistrationStyle>
        Register<TDependency1, TDependency2, TDependency3, TComponent> (
            this ContainerBuilder builder,
            Func<TDependency1, TDependency2, TDependency3, TComponent> @delegate)
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
    {
        if (@delegate is null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        var delegateInvoker = new DelegateInvokers.DelegateInvoker3<TDependency1, TDependency2, TDependency3, TComponent>(@delegate);

        return builder.Register(delegateInvoker.ResolveWithDelegate);
    }

    /// <summary>
    /// Register a delegate as a component.
    /// </summary>
    /// <typeparam name="TDependency1">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency2">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency3">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TComponent">The type of the instance.</typeparam>
    /// <param name="builder">Container builder.</param>
    /// <param name="delegate">The delegate to register.</param>
    /// <returns>Registration builder allowing the registration to be configured.</returns>
    public static IRegistrationBuilder<TComponent, SimpleActivatorData, SingleRegistrationStyle>
        Register<TDependency1, TDependency2, TDependency3, TComponent> (
            this ContainerBuilder builder,
            Func<IComponentContext, TDependency1, TDependency2, TDependency3, TComponent> @delegate)
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
    {
        if (@delegate is null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        var delegateInvoker = new DelegateInvokers.DelegateInvoker3WithComponentContext<TDependency1, TDependency2, TDependency3, TComponent>(@delegate);

        return builder.Register(delegateInvoker.ResolveWithDelegate);
    }

    /// <summary>
    /// Register a delegate as a component.
    /// </summary>
    /// <typeparam name="TDependency1">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency2">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency3">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency4">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TComponent">The type of the instance.</typeparam>
    /// <param name="builder">Container builder.</param>
    /// <param name="delegate">The delegate to register.</param>
    /// <returns>Registration builder allowing the registration to be configured.</returns>
    public static IRegistrationBuilder<TComponent, SimpleActivatorData, SingleRegistrationStyle>
        Register<TDependency1, TDependency2, TDependency3, TDependency4, TComponent> (
            this ContainerBuilder builder,
            Func<TDependency1, TDependency2, TDependency3, TDependency4, TComponent> @delegate)
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
    {
        if (@delegate is null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        var delegateInvoker = new DelegateInvokers.DelegateInvoker4<TDependency1, TDependency2, TDependency3, TDependency4, TComponent>(@delegate);

        return builder.Register(delegateInvoker.ResolveWithDelegate);
    }

    /// <summary>
    /// Register a delegate as a component.
    /// </summary>
    /// <typeparam name="TDependency1">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency2">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency3">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency4">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TComponent">The type of the instance.</typeparam>
    /// <param name="builder">Container builder.</param>
    /// <param name="delegate">The delegate to register.</param>
    /// <returns>Registration builder allowing the registration to be configured.</returns>
    public static IRegistrationBuilder<TComponent, SimpleActivatorData, SingleRegistrationStyle>
        Register<TDependency1, TDependency2, TDependency3, TDependency4, TComponent> (
            this ContainerBuilder builder,
            Func<IComponentContext, TDependency1, TDependency2, TDependency3, TDependency4, TComponent> @delegate)
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
    {
        if (@delegate is null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        var delegateInvoker = new DelegateInvokers.DelegateInvoker4WithComponentContext<TDependency1, TDependency2, TDependency3, TDependency4, TComponent>(@delegate);

        return builder.Register(delegateInvoker.ResolveWithDelegate);
    }

    /// <summary>
    /// Register a delegate as a component.
    /// </summary>
    /// <typeparam name="TDependency1">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency2">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency3">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency4">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency5">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TComponent">The type of the instance.</typeparam>
    /// <param name="builder">Container builder.</param>
    /// <param name="delegate">The delegate to register.</param>
    /// <returns>Registration builder allowing the registration to be configured.</returns>
    public static IRegistrationBuilder<TComponent, SimpleActivatorData, SingleRegistrationStyle>
        Register<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TComponent> (
            this ContainerBuilder builder,
            Func<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TComponent> @delegate)
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
        where TDependency5 : notnull
    {
        if (@delegate is null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        var delegateInvoker = new DelegateInvokers.DelegateInvoker5<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TComponent>(@delegate);

        return builder.Register(delegateInvoker.ResolveWithDelegate);
    }

    /// <summary>
    /// Register a delegate as a component.
    /// </summary>
    /// <typeparam name="TDependency1">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency2">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency3">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency4">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency5">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TComponent">The type of the instance.</typeparam>
    /// <param name="builder">Container builder.</param>
    /// <param name="delegate">The delegate to register.</param>
    /// <returns>Registration builder allowing the registration to be configured.</returns>
    public static IRegistrationBuilder<TComponent, SimpleActivatorData, SingleRegistrationStyle>
        Register<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TComponent> (
            this ContainerBuilder builder,
            Func<IComponentContext, TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TComponent> @delegate)
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
        where TDependency5 : notnull
    {
        if (@delegate is null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        var delegateInvoker = new DelegateInvokers.DelegateInvoker5WithComponentContext<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TComponent>(@delegate);

        return builder.Register(delegateInvoker.ResolveWithDelegate);
    }

    /// <summary>
    /// Register a delegate as a component.
    /// </summary>
    /// <typeparam name="TDependency1">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency2">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency3">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency4">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency5">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency6">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TComponent">The type of the instance.</typeparam>
    /// <param name="builder">Container builder.</param>
    /// <param name="delegate">The delegate to register.</param>
    /// <returns>Registration builder allowing the registration to be configured.</returns>
    public static IRegistrationBuilder<TComponent, SimpleActivatorData, SingleRegistrationStyle>
        Register<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TComponent> (
            this ContainerBuilder builder,
            Func<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TComponent> @delegate)
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
        where TDependency5 : notnull
        where TDependency6 : notnull
    {
        if (@delegate is null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        var delegateInvoker = new DelegateInvokers.DelegateInvoker6<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TComponent>(@delegate);

        return builder.Register(delegateInvoker.ResolveWithDelegate);
    }

    /// <summary>
    /// Register a delegate as a component.
    /// </summary>
    /// <typeparam name="TDependency1">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency2">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency3">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency4">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency5">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency6">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TComponent">The type of the instance.</typeparam>
    /// <param name="builder">Container builder.</param>
    /// <param name="delegate">The delegate to register.</param>
    /// <returns>Registration builder allowing the registration to be configured.</returns>
    public static IRegistrationBuilder<TComponent, SimpleActivatorData, SingleRegistrationStyle>
        Register<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TComponent> (
            this ContainerBuilder builder,
            Func<IComponentContext, TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TComponent> @delegate)
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
        where TDependency5 : notnull
        where TDependency6 : notnull
    {
        if (@delegate is null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        var delegateInvoker = new DelegateInvokers.DelegateInvoker6WithComponentContext<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TComponent>(@delegate);

        return builder.Register(delegateInvoker.ResolveWithDelegate);
    }

    /// <summary>
    /// Register a delegate as a component.
    /// </summary>
    /// <typeparam name="TDependency1">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency2">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency3">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency4">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency5">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency6">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency7">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TComponent">The type of the instance.</typeparam>
    /// <param name="builder">Container builder.</param>
    /// <param name="delegate">The delegate to register.</param>
    /// <returns>Registration builder allowing the registration to be configured.</returns>
    public static IRegistrationBuilder<TComponent, SimpleActivatorData, SingleRegistrationStyle>
        Register<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TComponent> (
            this ContainerBuilder builder,
            Func<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TComponent> @delegate)
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
        where TDependency5 : notnull
        where TDependency6 : notnull
        where TDependency7 : notnull
    {
        if (@delegate is null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        var delegateInvoker = new DelegateInvokers.DelegateInvoker7<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TComponent>(@delegate);

        return builder.Register(delegateInvoker.ResolveWithDelegate);
    }

    /// <summary>
    /// Register a delegate as a component.
    /// </summary>
    /// <typeparam name="TDependency1">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency2">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency3">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency4">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency5">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency6">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency7">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TComponent">The type of the instance.</typeparam>
    /// <param name="builder">Container builder.</param>
    /// <param name="delegate">The delegate to register.</param>
    /// <returns>Registration builder allowing the registration to be configured.</returns>
    public static IRegistrationBuilder<TComponent, SimpleActivatorData, SingleRegistrationStyle>
        Register<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TComponent> (
            this ContainerBuilder builder,
            Func<IComponentContext, TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TComponent> @delegate)
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
        where TDependency5 : notnull
        where TDependency6 : notnull
        where TDependency7 : notnull
    {
        if (@delegate is null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        var delegateInvoker = new DelegateInvokers.DelegateInvoker7WithComponentContext<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TComponent>(@delegate);

        return builder.Register(delegateInvoker.ResolveWithDelegate);
    }

    /// <summary>
    /// Register a delegate as a component.
    /// </summary>
    /// <typeparam name="TDependency1">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency2">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency3">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency4">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency5">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency6">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency7">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency8">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TComponent">The type of the instance.</typeparam>
    /// <param name="builder">Container builder.</param>
    /// <param name="delegate">The delegate to register.</param>
    /// <returns>Registration builder allowing the registration to be configured.</returns>
    public static IRegistrationBuilder<TComponent, SimpleActivatorData, SingleRegistrationStyle>
        Register<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TDependency8, TComponent> (
            this ContainerBuilder builder,
            Func<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TDependency8, TComponent> @delegate)
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
        where TDependency5 : notnull
        where TDependency6 : notnull
        where TDependency7 : notnull
        where TDependency8 : notnull
    {
        if (@delegate is null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        var delegateInvoker = new DelegateInvokers.DelegateInvoker8<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TDependency8, TComponent>(@delegate);

        return builder.Register(delegateInvoker.ResolveWithDelegate);
    }

    /// <summary>
    /// Register a delegate as a component.
    /// </summary>
    /// <typeparam name="TDependency1">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency2">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency3">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency4">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency5">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency6">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency7">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency8">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TComponent">The type of the instance.</typeparam>
    /// <param name="builder">Container builder.</param>
    /// <param name="delegate">The delegate to register.</param>
    /// <returns>Registration builder allowing the registration to be configured.</returns>
    public static IRegistrationBuilder<TComponent, SimpleActivatorData, SingleRegistrationStyle>
        Register<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TDependency8, TComponent> (
            this ContainerBuilder builder,
            Func<IComponentContext, TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TDependency8, TComponent> @delegate)
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
        where TDependency5 : notnull
        where TDependency6 : notnull
        where TDependency7 : notnull
        where TDependency8 : notnull
    {
        if (@delegate is null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        var delegateInvoker = new DelegateInvokers.DelegateInvoker8WithComponentContext<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TDependency8, TComponent>(@delegate);

        return builder.Register(delegateInvoker.ResolveWithDelegate);
    }

    /// <summary>
    /// Register a delegate as a component.
    /// </summary>
    /// <typeparam name="TDependency1">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency2">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency3">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency4">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency5">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency6">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency7">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency8">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency9">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TComponent">The type of the instance.</typeparam>
    /// <param name="builder">Container builder.</param>
    /// <param name="delegate">The delegate to register.</param>
    /// <returns>Registration builder allowing the registration to be configured.</returns>
    public static IRegistrationBuilder<TComponent, SimpleActivatorData, SingleRegistrationStyle>
        Register<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TDependency8, TDependency9, TComponent> (
            this ContainerBuilder builder,
            Func<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TDependency8, TDependency9, TComponent> @delegate)
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
        where TDependency5 : notnull
        where TDependency6 : notnull
        where TDependency7 : notnull
        where TDependency8 : notnull
        where TDependency9 : notnull
    {
        if (@delegate is null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        var delegateInvoker = new DelegateInvokers.DelegateInvoker9<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TDependency8, TDependency9, TComponent>(@delegate);

        return builder.Register(delegateInvoker.ResolveWithDelegate);
    }

    /// <summary>
    /// Register a delegate as a component.
    /// </summary>
    /// <typeparam name="TDependency1">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency2">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency3">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency4">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency5">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency6">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency7">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency8">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency9">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TComponent">The type of the instance.</typeparam>
    /// <param name="builder">Container builder.</param>
    /// <param name="delegate">The delegate to register.</param>
    /// <returns>Registration builder allowing the registration to be configured.</returns>
    public static IRegistrationBuilder<TComponent, SimpleActivatorData, SingleRegistrationStyle>
        Register<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TDependency8, TDependency9, TComponent> (
            this ContainerBuilder builder,
            Func<IComponentContext, TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TDependency8, TDependency9, TComponent> @delegate)
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
        where TDependency5 : notnull
        where TDependency6 : notnull
        where TDependency7 : notnull
        where TDependency8 : notnull
        where TDependency9 : notnull
    {
        if (@delegate is null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        var delegateInvoker = new DelegateInvokers.DelegateInvoker9WithComponentContext<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TDependency8, TDependency9, TComponent>(@delegate);

        return builder.Register(delegateInvoker.ResolveWithDelegate);
    }

    /// <summary>
    /// Register a delegate as a component.
    /// </summary>
    /// <typeparam name="TDependency1">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency2">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency3">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency4">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency5">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency6">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency7">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency8">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency9">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency10">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TComponent">The type of the instance.</typeparam>
    /// <param name="builder">Container builder.</param>
    /// <param name="delegate">The delegate to register.</param>
    /// <returns>Registration builder allowing the registration to be configured.</returns>
    public static IRegistrationBuilder<TComponent, SimpleActivatorData, SingleRegistrationStyle>
        Register<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TDependency8, TDependency9, TDependency10, TComponent> (
            this ContainerBuilder builder,
            Func<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TDependency8, TDependency9, TDependency10, TComponent> @delegate)
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
        where TDependency5 : notnull
        where TDependency6 : notnull
        where TDependency7 : notnull
        where TDependency8 : notnull
        where TDependency9 : notnull
        where TDependency10 : notnull
    {
        if (@delegate is null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        var delegateInvoker = new DelegateInvokers.DelegateInvoker10<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TDependency8, TDependency9, TDependency10, TComponent>(@delegate);

        return builder.Register(delegateInvoker.ResolveWithDelegate);
    }

    /// <summary>
    /// Register a delegate as a component.
    /// </summary>
    /// <typeparam name="TDependency1">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency2">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency3">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency4">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency5">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency6">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency7">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency8">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency9">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TDependency10">The type of a dependency to inject into the delegate.</typeparam>
    /// <typeparam name="TComponent">The type of the instance.</typeparam>
    /// <param name="builder">Container builder.</param>
    /// <param name="delegate">The delegate to register.</param>
    /// <returns>Registration builder allowing the registration to be configured.</returns>
    public static IRegistrationBuilder<TComponent, SimpleActivatorData, SingleRegistrationStyle>
        Register<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TDependency8, TDependency9, TDependency10, TComponent> (
            this ContainerBuilder builder,
            Func<IComponentContext, TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TDependency8, TDependency9, TDependency10, TComponent> @delegate)
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
        where TDependency5 : notnull
        where TDependency6 : notnull
        where TDependency7 : notnull
        where TDependency8 : notnull
        where TDependency9 : notnull
        where TDependency10 : notnull
    {
        if (@delegate is null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        var delegateInvoker = new DelegateInvokers.DelegateInvoker10WithComponentContext<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TDependency8, TDependency9, TDependency10, TComponent>(@delegate);

        return builder.Register(delegateInvoker.ResolveWithDelegate);
    }

}
