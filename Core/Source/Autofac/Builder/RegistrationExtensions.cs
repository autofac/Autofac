// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using Autofac.Core;
using Autofac.Features.Collections;
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
            if (delegateType == null) throw new ArgumentNullException("delegateType");
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
            if (builder == null) throw new ArgumentNullException("builder");
            Enforce.ArgumentTypeIsFunction(typeof(TDelegate));

            var returnType = typeof(TDelegate).FunctionReturnType();
            return builder.RegisterGeneratedFactory<TDelegate>(new TypedService(returnType));
        }

        /// <summary>
        /// Changes the parameter mapping mode of the supplied delegate type to match
        /// parameters by name.
        /// </summary>
        /// <typeparam name="TDelegate">Factory delegate type</typeparam>
        /// <typeparam name="TGeneratedFactoryActivatorData">Activator data type</typeparam>
        /// <typeparam name="TSingleRegistrationStyle">Registration style</typeparam>
        /// <param name="registration">Registration to change parameter mapping mode of.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle>
            NamedParameterMapping<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle>(
                this IRegistrationBuilder<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle> registration)
            where TGeneratedFactoryActivatorData : GeneratedFactoryActivatorData
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            registration.ActivatorData.ParameterMapping = ParameterMapping.ByName;
            return registration;
        }

        /// <summary>
        /// Changes the parameter mapping mode of the supplied delegate type to match
        /// parameters by position.
        /// </summary>
        /// <typeparam name="TDelegate">Factory delegate type</typeparam>
        /// <typeparam name="TGeneratedFactoryActivatorData">Activator data type</typeparam>
        /// <typeparam name="TSingleRegistrationStyle">Registration style</typeparam>
        /// <param name="registration">Registration to change parameter mapping mode of.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle>
            PositionalParameterMapping<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle>(
                this IRegistrationBuilder<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle> registration)
            where TGeneratedFactoryActivatorData : GeneratedFactoryActivatorData
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            registration.ActivatorData.ParameterMapping = ParameterMapping.ByPosition;
            return registration;
        }

        /// <summary>
        /// Changes the parameter mapping mode of the supplied delegate type to match
        /// parameters by type.
        /// </summary>
        /// <typeparam name="TDelegate">Factory delegate type</typeparam>
        /// <typeparam name="TGeneratedFactoryActivatorData">Activator data type</typeparam>
        /// <typeparam name="TSingleRegistrationStyle">Registration style</typeparam>
        /// <param name="registration">Registration to change parameter mapping mode of.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle>
            TypedParameterMapping<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle>(
                this IRegistrationBuilder<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle> registration)
            where TGeneratedFactoryActivatorData : GeneratedFactoryActivatorData
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            registration.ActivatorData.ParameterMapping = ParameterMapping.ByType;
            return registration;
        }

        /// <summary>
        /// Registers the type as a collection. If no services or names are specified, the
        /// default services will be IList&lt;T&gt;, ICollection&lt;T&gt;, and IEnumerable&lt;T&gt;        
        /// </summary>
        /// <param name="elementType">The type of the collection elements.</param>
        /// <param name="builder">Container builder.</param>
        /// <param name="collectionName">A unique name for the collection that can be passed to MemberOf().</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        /// <remarks>Collections are provided automatically in Autofac 2,
        /// and this method is generally not required.</remarks>
        public static IRegistrationBuilder<object[], SimpleActivatorData, SingleRegistrationStyle>
            RegisterCollection(this ContainerBuilder builder, string collectionName, Type elementType)
        {
            return CollectionRegistrationExtensions.RegisterCollection<object>(builder, collectionName, elementType);
        }

        /// <summary>
        /// Registers the type as a collection. If no services or names are specified, the
        /// default services will be IList&lt;T&gt;, ICollection&lt;T&gt;, and IEnumerable&lt;T&gt;        
        /// </summary>
        /// <typeparam name="T">The type of the collection elements.</typeparam>
        /// <param name="builder">Container builder.</param>
        /// <param name="collectionName">A unique name for the collection that can be passed to MemberOf().</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        /// <remarks>Collections are provided automatically in Autofac 2,
        /// and this method is generally not required.</remarks>
        public static IRegistrationBuilder<T[], SimpleActivatorData, SingleRegistrationStyle>
            RegisterCollection<T>(this ContainerBuilder builder, string collectionName)
        {
            return CollectionRegistrationExtensions.RegisterCollection<T>(builder, collectionName, typeof(T));
        }

        /// <summary>
        /// Include the element explicitly in a collection configured using RegisterCollection.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TSingleRegistrationStyle">Registration style.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <param name="registration">Registration to export.</param>
        /// <param name="collectionName">The collection name, as passed to RegisterCollection.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle>
            MemberOf<TLimit, TActivatorData, TSingleRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> registration,
                string collectionName)
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            return CollectionRegistrationExtensions.MemberOf(registration, collectionName);
        }
    }
}
