// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Activators.ProvidedInstance;
using Autofac.Core.Activators.Reflection;
using Autofac.Core.Lifetime;
using Autofac.Core.Resolving.Pipeline;
using Autofac.Features.Scanning;
using Autofac.Util;

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
        Register<TDependency1, TComponent>(
            this ContainerBuilder builder,
            Func<IComponentContext, TDependency1, TComponent> @delegate)
        where TDependency1 : notnull
        where TComponent : notnull
    {
        if (@delegate == null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        return builder.Register((c, p) => @delegate(
            c,
            c.Resolve<TDependency1>()));
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
        Register<TDependency1, TComponent>(
            this ContainerBuilder builder,
            Func<TDependency1, TComponent> @delegate)
        where TDependency1 : notnull
        where TComponent : notnull
    {
        if (@delegate == null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        return builder.Register((c, p) => @delegate(
            c.Resolve<TDependency1>()));
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
        Register<TDependency1, TDependency2, TComponent>(
            this ContainerBuilder builder,
            Func<IComponentContext, TDependency1, TDependency2, TComponent> @delegate)
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TComponent : notnull
    {
        if (@delegate == null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        return builder.Register((c, p) => @delegate(
            c,
            c.Resolve<TDependency1>(),
            c.Resolve<TDependency2>()));
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
        Register<TDependency1, TDependency2, TComponent>(
            this ContainerBuilder builder,
            Func<TDependency1, TDependency2, TComponent> @delegate)
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TComponent : notnull
    {
        if (@delegate == null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        return builder.Register((c, p) => @delegate(
            c.Resolve<TDependency1>(),
            c.Resolve<TDependency2>()));
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
        Register<TDependency1, TDependency2, TDependency3, TComponent>(
            this ContainerBuilder builder,
            Func<IComponentContext, TDependency1, TDependency2, TDependency3, TComponent> @delegate)
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TComponent : notnull
    {
        if (@delegate == null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        return builder.Register((c, p) => @delegate(
            c,
            c.Resolve<TDependency1>(),
            c.Resolve<TDependency2>(),
            c.Resolve<TDependency3>()));
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
        Register<TDependency1, TDependency2, TDependency3, TComponent>(
            this ContainerBuilder builder,
            Func<TDependency1, TDependency2, TDependency3, TComponent> @delegate)
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TComponent : notnull
    {
        if (@delegate == null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        return builder.Register((c, p) => @delegate(
            c.Resolve<TDependency1>(),
            c.Resolve<TDependency2>(),
            c.Resolve<TDependency3>()));
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
        Register<TDependency1, TDependency2, TDependency3, TDependency4, TComponent>(
            this ContainerBuilder builder,
            Func<IComponentContext, TDependency1, TDependency2, TDependency3, TDependency4, TComponent> @delegate)
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
        where TComponent : notnull
    {
        if (@delegate == null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        return builder.Register((c, p) => @delegate(
            c,
            c.Resolve<TDependency1>(),
            c.Resolve<TDependency2>(),
            c.Resolve<TDependency3>(),
            c.Resolve<TDependency4>()));
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
        Register<TDependency1, TDependency2, TDependency3, TDependency4, TComponent>(
            this ContainerBuilder builder,
            Func<TDependency1, TDependency2, TDependency3, TDependency4, TComponent> @delegate)
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
        where TComponent : notnull
    {
        if (@delegate == null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        return builder.Register((c, p) => @delegate(
            c.Resolve<TDependency1>(),
            c.Resolve<TDependency2>(),
            c.Resolve<TDependency3>(),
            c.Resolve<TDependency4>()));
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
        Register<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TComponent>(
            this ContainerBuilder builder,
            Func<IComponentContext, TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TComponent> @delegate)
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
        where TDependency5 : notnull
        where TComponent : notnull
    {
        if (@delegate == null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        return builder.Register((c, p) => @delegate(
            c,
            c.Resolve<TDependency1>(),
            c.Resolve<TDependency2>(),
            c.Resolve<TDependency3>(),
            c.Resolve<TDependency4>(),
            c.Resolve<TDependency5>()));
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
        Register<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TComponent>(
            this ContainerBuilder builder,
            Func<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TComponent> @delegate)
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
        where TDependency5 : notnull
        where TComponent : notnull
    {
        if (@delegate == null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        return builder.Register((c, p) => @delegate(
            c.Resolve<TDependency1>(),
            c.Resolve<TDependency2>(),
            c.Resolve<TDependency3>(),
            c.Resolve<TDependency4>(),
            c.Resolve<TDependency5>()));
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
        Register<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TComponent>(
            this ContainerBuilder builder,
            Func<IComponentContext, TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TComponent> @delegate)
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
        where TDependency5 : notnull
        where TDependency6 : notnull
        where TComponent : notnull
    {
        if (@delegate == null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        return builder.Register((c, p) => @delegate(
            c,
            c.Resolve<TDependency1>(),
            c.Resolve<TDependency2>(),
            c.Resolve<TDependency3>(),
            c.Resolve<TDependency4>(),
            c.Resolve<TDependency5>(),
            c.Resolve<TDependency6>()));
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
        Register<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TComponent>(
            this ContainerBuilder builder,
            Func<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TComponent> @delegate)
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
        where TDependency5 : notnull
        where TDependency6 : notnull
        where TComponent : notnull
    {
        if (@delegate == null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        return builder.Register((c, p) => @delegate(
            c.Resolve<TDependency1>(),
            c.Resolve<TDependency2>(),
            c.Resolve<TDependency3>(),
            c.Resolve<TDependency4>(),
            c.Resolve<TDependency5>(),
            c.Resolve<TDependency6>()));
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
        Register<TDependency1, TDependency2, TDependency3, TDependency4,
                 TDependency5, TDependency6, TDependency7, TComponent>(
            this ContainerBuilder builder,
            Func<IComponentContext,
                 TDependency1, TDependency2, TDependency3, TDependency4,
                 TDependency5, TDependency6, TDependency7,
                 TComponent> @delegate)
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
        where TDependency5 : notnull
        where TDependency6 : notnull
        where TDependency7 : notnull
        where TComponent : notnull
    {
        if (@delegate == null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        return builder.Register((c, p) => @delegate(
            c,
            c.Resolve<TDependency1>(),
            c.Resolve<TDependency2>(),
            c.Resolve<TDependency3>(),
            c.Resolve<TDependency4>(),
            c.Resolve<TDependency5>(),
            c.Resolve<TDependency6>(),
            c.Resolve<TDependency7>()));
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
        Register<TDependency1, TDependency2, TDependency3, TDependency4,
                 TDependency5, TDependency6, TDependency7, TComponent>(
            this ContainerBuilder builder,
            Func<TDependency1, TDependency2, TDependency3, TDependency4,
                 TDependency5, TDependency6, TDependency7, TComponent> @delegate)
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
        where TDependency5 : notnull
        where TDependency6 : notnull
        where TDependency7 : notnull
        where TComponent : notnull
    {
        if (@delegate == null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        return builder.Register((c, p) => @delegate(
            c.Resolve<TDependency1>(),
            c.Resolve<TDependency2>(),
            c.Resolve<TDependency3>(),
            c.Resolve<TDependency4>(),
            c.Resolve<TDependency5>(),
            c.Resolve<TDependency6>(),
            c.Resolve<TDependency7>()));
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
        Register<TDependency1, TDependency2, TDependency3, TDependency4,
                 TDependency5, TDependency6, TDependency7, TDependency8,
                 TComponent>(
            this ContainerBuilder builder,
            Func<IComponentContext,
                 TDependency1, TDependency2, TDependency3, TDependency4,
                 TDependency5, TDependency6, TDependency7, TDependency8,
                 TComponent> @delegate)
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
        where TDependency5 : notnull
        where TDependency6 : notnull
        where TDependency7 : notnull
        where TDependency8 : notnull
        where TComponent : notnull
    {
        if (@delegate == null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        return builder.Register((c, p) => @delegate(
            c,
            c.Resolve<TDependency1>(),
            c.Resolve<TDependency2>(),
            c.Resolve<TDependency3>(),
            c.Resolve<TDependency4>(),
            c.Resolve<TDependency5>(),
            c.Resolve<TDependency6>(),
            c.Resolve<TDependency7>(),
            c.Resolve<TDependency8>()));
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
        Register<TDependency1, TDependency2, TDependency3, TDependency4,
                 TDependency5, TDependency6, TDependency7, TDependency8,
                 TComponent>(
            this ContainerBuilder builder,
            Func<TDependency1, TDependency2, TDependency3, TDependency4,
                 TDependency5, TDependency6, TDependency7, TDependency8,
                 TComponent> @delegate)
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
        where TDependency5 : notnull
        where TDependency6 : notnull
        where TDependency7 : notnull
        where TDependency8 : notnull
        where TComponent : notnull
    {
        if (@delegate == null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        return builder.Register((c, p) => @delegate(
            c.Resolve<TDependency1>(),
            c.Resolve<TDependency2>(),
            c.Resolve<TDependency3>(),
            c.Resolve<TDependency4>(),
            c.Resolve<TDependency5>(),
            c.Resolve<TDependency6>(),
            c.Resolve<TDependency7>(),
            c.Resolve<TDependency8>()));
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
        Register<TDependency1, TDependency2, TDependency3, TDependency4,
                 TDependency5, TDependency6, TDependency7, TDependency8,
                 TDependency9, TComponent>(
            this ContainerBuilder builder,
            Func<IComponentContext,
                 TDependency1, TDependency2, TDependency3, TDependency4,
                 TDependency5, TDependency6, TDependency7, TDependency8,
                 TDependency9, TComponent> @delegate)
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
        where TDependency5 : notnull
        where TDependency6 : notnull
        where TDependency7 : notnull
        where TDependency8 : notnull
        where TDependency9 : notnull
        where TComponent : notnull
    {
        if (@delegate == null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        return builder.Register((c, p) => @delegate(
            c,
            c.Resolve<TDependency1>(),
            c.Resolve<TDependency2>(),
            c.Resolve<TDependency3>(),
            c.Resolve<TDependency4>(),
            c.Resolve<TDependency5>(),
            c.Resolve<TDependency6>(),
            c.Resolve<TDependency7>(),
            c.Resolve<TDependency8>(),
            c.Resolve<TDependency9>()));
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
        Register<TDependency1, TDependency2, TDependency3, TDependency4,
                 TDependency5, TDependency6, TDependency7, TDependency8,
                 TDependency9, TComponent>(
            this ContainerBuilder builder,
            Func<TDependency1, TDependency2, TDependency3, TDependency4,
                 TDependency5, TDependency6, TDependency7, TDependency8,
                 TDependency9, TComponent> @delegate)
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
        where TDependency5 : notnull
        where TDependency6 : notnull
        where TDependency7 : notnull
        where TDependency8 : notnull
        where TDependency9 : notnull
        where TComponent : notnull
    {
        if (@delegate == null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        return builder.Register((c, p) => @delegate(
            c.Resolve<TDependency1>(),
            c.Resolve<TDependency2>(),
            c.Resolve<TDependency3>(),
            c.Resolve<TDependency4>(),
            c.Resolve<TDependency5>(),
            c.Resolve<TDependency6>(),
            c.Resolve<TDependency7>(),
            c.Resolve<TDependency8>(),
            c.Resolve<TDependency9>()));
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
        Register<TDependency1, TDependency2, TDependency3, TDependency4,
                 TDependency5, TDependency6, TDependency7, TDependency8,
                 TDependency9, TDependency10, TComponent>(
            this ContainerBuilder builder,
            Func<IComponentContext,
                 TDependency1, TDependency2, TDependency3, TDependency4,
                 TDependency5, TDependency6, TDependency7, TDependency8,
                 TDependency9, TDependency10, TComponent> @delegate)
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
        where TComponent : notnull
    {
        if (@delegate == null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        return builder.Register((c, p) => @delegate(
            c,
            c.Resolve<TDependency1>(),
            c.Resolve<TDependency2>(),
            c.Resolve<TDependency3>(),
            c.Resolve<TDependency4>(),
            c.Resolve<TDependency5>(),
            c.Resolve<TDependency6>(),
            c.Resolve<TDependency7>(),
            c.Resolve<TDependency8>(),
            c.Resolve<TDependency9>(),
            c.Resolve<TDependency10>()));
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
        Register<TDependency1, TDependency2, TDependency3, TDependency4,
                 TDependency5, TDependency6, TDependency7, TDependency8,
                 TDependency9, TDependency10, TComponent>(
            this ContainerBuilder builder,
            Func<TDependency1, TDependency2, TDependency3, TDependency4,
                 TDependency5, TDependency6, TDependency7, TDependency8,
                 TDependency9, TDependency10, TComponent> @delegate)
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
        where TComponent : notnull
    {
        if (@delegate == null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        return builder.Register((c, p) => @delegate(
            c.Resolve<TDependency1>(),
            c.Resolve<TDependency2>(),
            c.Resolve<TDependency3>(),
            c.Resolve<TDependency4>(),
            c.Resolve<TDependency5>(),
            c.Resolve<TDependency6>(),
            c.Resolve<TDependency7>(),
            c.Resolve<TDependency8>(),
            c.Resolve<TDependency9>(),
            c.Resolve<TDependency10>()));
    }
}
