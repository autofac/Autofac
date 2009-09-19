// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2008 Autofac Contributors
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Core.Activators;
using Autofac.GeneratedFactories;
using Autofac.Core.Activators.Delegate;

namespace Autofac.Builder
{
    /// <summary>
    /// Data used to create factory activators.
    /// </summary>
    public class GeneratedFactoryActivatorData
    {
        ParameterMapping _parameterMapping = ParameterMapping.Adaptive;

        /// <summary>
        /// Determines how the parameters of the delegate type are passed on
        /// to the generated Resolve() call as Parameter objects.
        /// For Func-based delegates, this defaults to ByType. Otherwise, the
        /// parameters will be mapped by name.
        /// </summary>
        public ParameterMapping ParameterMapping
        {
            get { return _parameterMapping; }
            set { _parameterMapping = value; }
        }
    }

    /// <summary>
    /// ContainerBuilder extensions for registering generated factories.
    /// </summary>
    public static class GeneratedFactoryRegistrationExtensions
    {
        /// <summary>
        /// Registers a factory delegate.
        /// </summary>
        /// <param name="builder">Container builder.</param>
        /// <param name="delegateType">Factory type to generate.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static RegistrationBuilder<Delegate, GeneratedFactoryActivatorData, SingleRegistrationStyle>
            RegisterGeneratedFactory(this ContainerBuilder builder, Type delegateType)
        {
            Enforce.ArgumentNotNull(delegateType, "delegateType");
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
        public static RegistrationBuilder<Delegate, GeneratedFactoryActivatorData, SingleRegistrationStyle>
            RegisterGeneratedFactory(this ContainerBuilder builder, Type delegateType, Service service)
        {
            return RegisterGeneratedFactoryInternal<Delegate>(builder, delegateType, service);
        }

        /// <summary>
        /// Registers a factory delegate.
        /// </summary>
        /// <typeparam name="TDelegate">The type of the delegate.</typeparam>
        /// <param name="builder">Container builder.</param>
        /// <param name="service">The service that the delegate will return instances of.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static RegistrationBuilder<TDelegate, GeneratedFactoryActivatorData, SingleRegistrationStyle>
            RegisterGeneratedFactory<TDelegate>(this ContainerBuilder builder, Service service)
            where TDelegate : class
        {
            return RegisterGeneratedFactoryInternal<TDelegate>(builder, typeof(TDelegate), service);
        }

        /// <summary>
        /// Registers a factory delegate.
        /// </summary>
        /// <typeparam name="TDelegate">The type of the delegate.</typeparam>
        /// <param name="builder">Container builder.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static RegistrationBuilder<TDelegate, GeneratedFactoryActivatorData, SingleRegistrationStyle>
            RegisterGeneratedFactory<TDelegate>(this ContainerBuilder builder)
            where TDelegate : class
        {
            Enforce.ArgumentNotNull(builder, "builder");
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
        public static RegistrationBuilder<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle>
            NamedParameterMapping<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle>(
                this RegistrationBuilder<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle> registration)
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
        public static RegistrationBuilder<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle>
            PositionalParameterMapping<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle>(
                this RegistrationBuilder<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle> registration)
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
        public static  RegistrationBuilder<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle>
            TypedParameterMapping<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle>(
                this RegistrationBuilder<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle> registration)
            where TGeneratedFactoryActivatorData : GeneratedFactoryActivatorData
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            registration.ActivatorData.ParameterMapping = ParameterMapping.ByType;
            return registration;
        }

        static RegistrationBuilder<TLimit, GeneratedFactoryActivatorData, SingleRegistrationStyle>
            RegisterGeneratedFactoryInternal<TLimit>(ContainerBuilder builder, Type delegateType, Service service)
        {
            var activatorData = new GeneratedFactoryActivatorData();

            var rb = new RegistrationBuilder<TLimit, GeneratedFactoryActivatorData, SingleRegistrationStyle>(
                activatorData, new SingleRegistrationStyle());

            builder.RegisterCallback(cr => {
                var factory = new FactoryGenerator(delegateType, service, activatorData.ParameterMapping);
                var activator = new DelegateActivator(delegateType, (c, p) => factory.GenerateFactory(c, p));
                RegistrationExtensions.RegisterSingleComponent(cr, rb, activator);
            });

            return rb.InstancePerLifetimeScope();
        }
    }
}
