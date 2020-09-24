// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Core;
using Autofac.Features.GeneratedFactories;
using Autofac.Util;

namespace Autofac.Builder
{
    /// <summary>
    /// Adds registration syntax for less commonly-used features.
    /// </summary>
    /// <remarks>
    /// These features are in this namespace because they will remain accessible to
    /// applications originally written against Autofac 1.4. In Autofac 2, this functionality
    /// is implicitly provided and thus making explicit registrations is rarely necessary.
    /// </remarks>
    public static class RegistrationExtensions
    {
        /// <summary>
        /// Registers a factory delegate.
        /// </summary>
        /// <param name="builder">Container builder.</param>
        /// <param name="delegateType">Factory type to generate.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        /// <remarks>Factory delegates are provided automatically in Autofac 2,
        /// and this method is generally not required.</remarks>
        public static IRegistrationBuilder<Delegate, GeneratedFactoryActivatorData, SingleRegistrationStyle>
            RegisterGeneratedFactory(this ContainerBuilder builder, Type delegateType)
        {
            if (delegateType == null)
            {
                throw new ArgumentNullException(nameof(delegateType));
            }

            Enforce.ArgumentTypeIsFunction(delegateType);

            var returnType = delegateType.FunctionReturnType();
            return builder.RegisterGeneratedFactory(delegateType, new TypedService(returnType));
        }

        /// <summary>
        /// Registers a factory delegate.
        /// </summary>
        /// <param name="builder">Container builder.</param>
        /// <param name="delegateType">Factory type to generate.</param>
        /// <param name="service">The service that the delegate will return instances of.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        /// <remarks>Factory delegates are provided automatically in Autofac 2, and
        /// this method is generally not required.</remarks>
        public static IRegistrationBuilder<Delegate, GeneratedFactoryActivatorData, SingleRegistrationStyle>
            RegisterGeneratedFactory(this ContainerBuilder builder, Type delegateType, Service service)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return GeneratedFactoryRegistrationExtensions.RegisterGeneratedFactory<Delegate>(builder, delegateType, service);
        }

        /// <summary>
        /// Registers a factory delegate.
        /// </summary>
        /// <typeparam name="TDelegate">The type of the delegate.</typeparam>
        /// <param name="builder">Container builder.</param>
        /// <param name="service">The service that the delegate will return instances of.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        /// <remarks>Factory delegates are provided automatically in Autofac 2,
        /// and this method is generally not required.</remarks>
        public static IRegistrationBuilder<TDelegate, GeneratedFactoryActivatorData, SingleRegistrationStyle>
            RegisterGeneratedFactory<TDelegate>(this ContainerBuilder builder, Service service)
            where TDelegate : class
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return GeneratedFactoryRegistrationExtensions.RegisterGeneratedFactory<TDelegate>(builder, typeof(TDelegate), service);
        }

        /// <summary>
        /// Registers a factory delegate.
        /// </summary>
        /// <typeparam name="TDelegate">The type of the delegate.</typeparam>
        /// <param name="builder">Container builder.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        /// <remarks>Factory delegates are provided automatically in Autofac 2,
        /// and this method is generally not required.</remarks>
        public static IRegistrationBuilder<TDelegate, GeneratedFactoryActivatorData, SingleRegistrationStyle>
            RegisterGeneratedFactory<TDelegate>(this ContainerBuilder builder)
            where TDelegate : class
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            Enforce.ArgumentTypeIsFunction(typeof(TDelegate));

            var returnType = typeof(TDelegate).FunctionReturnType();
            return builder.RegisterGeneratedFactory<TDelegate>(new TypedService(returnType));
        }

        /// <summary>
        /// Changes the parameter mapping mode of the supplied delegate type to match
        /// parameters by name.
        /// </summary>
        /// <typeparam name="TDelegate">Factory delegate type.</typeparam>
        /// <typeparam name="TGeneratedFactoryActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TSingleRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to change parameter mapping mode of.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="registration" /> is <see langword="null" />.
        /// </exception>
        public static IRegistrationBuilder<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle>
            NamedParameterMapping<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle>(
                this IRegistrationBuilder<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle> registration)
            where TGeneratedFactoryActivatorData : GeneratedFactoryActivatorData
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            registration.ActivatorData.ParameterMapping = ParameterMapping.ByName;
            return registration;
        }

        /// <summary>
        /// Changes the parameter mapping mode of the supplied delegate type to match
        /// parameters by position.
        /// </summary>
        /// <typeparam name="TDelegate">Factory delegate type.</typeparam>
        /// <typeparam name="TGeneratedFactoryActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TSingleRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to change parameter mapping mode of.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="registration" /> is <see langword="null" />.
        /// </exception>
        public static IRegistrationBuilder<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle>
            PositionalParameterMapping<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle>(
                this IRegistrationBuilder<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle> registration)
            where TGeneratedFactoryActivatorData : GeneratedFactoryActivatorData
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            registration.ActivatorData.ParameterMapping = ParameterMapping.ByPosition;
            return registration;
        }

        /// <summary>
        /// Changes the parameter mapping mode of the supplied delegate type to match
        /// parameters by type.
        /// </summary>
        /// <typeparam name="TDelegate">Factory delegate type.</typeparam>
        /// <typeparam name="TGeneratedFactoryActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TSingleRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to change parameter mapping mode of.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="registration" /> is <see langword="null" />.
        /// </exception>
        public static IRegistrationBuilder<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle>
            TypedParameterMapping<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle>(
                this IRegistrationBuilder<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle> registration)
            where TGeneratedFactoryActivatorData : GeneratedFactoryActivatorData
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            registration.ActivatorData.ParameterMapping = ParameterMapping.ByType;
            return registration;
        }
    }
}
