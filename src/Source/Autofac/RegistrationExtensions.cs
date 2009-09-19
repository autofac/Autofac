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
using Autofac.Registration;
using Autofac.Lifetime;
using System.ComponentModel;
using Autofac.OpenGenerics;
using System.Globalization;
using Autofac.GeneratedFactories;
using Autofac.Core.Activators.Reflection;
using Autofac.Core;
using System.Reflection;
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Activators.ProvidedInstance;

namespace Autofac.Builder
{
    /// <summary>
    /// Adds registration syntax to the ContainerBuilder type.
    /// </summary>
    public static class RegistrationExtensions
    {
        /// <summary>
        /// Add a module to the container.
        /// </summary>
        /// <param name="builder">The builder to register the module with.</param>
        /// <param name="module">The module to add.</param>
        public static void RegisterModule(this ContainerBuilder builder, IModule module)
        {
            Enforce.ArgumentNotNull(module, "module");
            builder.RegisterCallback(cr => module.Configure(cr));
        }

        /// <summary>
        /// Add a component to the container.
        /// </summary>
        /// <param name="builder">The builder to register the component with.</param>
        /// <param name="registration">The component to add.</param>
        public static void RegisterComponent(this ContainerBuilder builder, IComponentRegistration registration)
        {
            Enforce.ArgumentNotNull(registration, "registration");
            builder.RegisterCallback(cr => cr.Register(registration));
        }

        /// <summary>
        /// Add a registration source to the container.
        /// </summary>
        /// <param name="builder">The builder to register the registration source via.</param>
        /// <param name="registrationSource">The registration source to add.</param>
        public static void RegisterSource(this ContainerBuilder builder, IRegistrationSource registrationSource)
        {
            Enforce.ArgumentNotNull(registrationSource, "registrationSource");
            builder.RegisterCallback(cr => cr.AddRegistrationSource(registrationSource));
        }

        /// <summary>
        /// Register an instance as a component.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="builder">Container builder.</param>
        /// <param name="instance">The instance to register.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static RegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle>
            RegisterInstance<T>(this ContainerBuilder builder, T instance)
            where T : class
        {
            Enforce.ArgumentNotNull(builder, "builder");
            Enforce.ArgumentNotNull(instance, "instance");

            var rb = new RegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle>(
                new SimpleActivatorData(new ProvidedInstanceActivator(instance)),
                new SingleRegistrationStyle());

            rb.SingleSharedInstance();

            builder.RegisterCallback(cr => RegisterSingleComponent(cr, rb, rb.ActivatorData.Activator));

            return rb;
        }

        /// <summary>
        /// Register a component to be created through reflection.
        /// </summary>
        /// <typeparam name="TImplementor">The type of the component implementation.</typeparam>
        /// <param name="builder">Container builder.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static RegistrationBuilder<TImplementor, ReflectionActivatorData, SingleRegistrationStyle>
            RegisterType<TImplementor>(this ContainerBuilder builder)
        {
            Enforce.ArgumentNotNull(builder, "builder");

            var rb = new RegistrationBuilder<TImplementor, ReflectionActivatorData, SingleRegistrationStyle>(
                new ReflectionActivatorData(typeof(TImplementor)),
                new SingleRegistrationStyle());

            builder.RegisterCallback(cr => RegisterSingleComponent(cr, rb, 
                new ReflectionActivator(
                    rb.ActivatorData.ImplementationType,
                    rb.ActivatorData.ConstructorFinder,
                    rb.ActivatorData.ConstructorSelector,
                    rb.ActivatorData.ConfiguredParameters)));

            return rb;
        }

        /// <summary>
        /// Register a component to be created through reflection.
        /// </summary>
        /// <param name="implementationType">The type of the component implementation.</param>
        /// <param name="builder">Container builder.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static RegistrationBuilder<object, ReflectionActivatorData, SingleRegistrationStyle>
            RegisterType(this ContainerBuilder builder, Type implementationType)
        {
            Enforce.ArgumentNotNull(builder, "builder");
            Enforce.ArgumentNotNull(implementationType, "implementationType");

            var rb = new RegistrationBuilder<object, ReflectionActivatorData, SingleRegistrationStyle>(
                new ReflectionActivatorData(implementationType),
                new SingleRegistrationStyle());

            builder.RegisterCallback(cr => RegisterSingleComponent(cr, rb,
                new ReflectionActivator(
                    rb.ActivatorData.ImplementationType,
                    rb.ActivatorData.ConstructorFinder,
                    rb.ActivatorData.ConstructorSelector,
                    rb.ActivatorData.ConfiguredParameters)));

            return rb;
        }

        /// <summary>
        /// Register a delegate as a component.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="builder">Container builder.</param>
        /// <param name="delegate">The delegate to register.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static RegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle>
            RegisterDelegate<T>(
                this ContainerBuilder builder,
                Func<IComponentContext, T> @delegate)
        {
            Enforce.ArgumentNotNull(@delegate, "delegate");
            return builder.RegisterDelegate<T>((c, p) => @delegate(c));
        }

        /// <summary>
        /// Register a delegate as a component.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="builder">Container builder.</param>
        /// <param name="delegate">The delegate to register.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static RegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle>
            RegisterDelegate<T>(
                this ContainerBuilder builder,
                Func<IComponentContext, IEnumerable<Parameter>, T> @delegate)
        {
            Enforce.ArgumentNotNull(builder, "builder");
            Enforce.ArgumentNotNull(@delegate, "delegate");

            var rb = new RegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle>(
                new SimpleActivatorData(new DelegateActivator(typeof(T), (c, p) => @delegate(c, p))),
                new SingleRegistrationStyle());

            builder.RegisterCallback(cr => RegisterSingleComponent(cr, rb, rb.ActivatorData.Activator));

            return rb;
        }

        /// <summary>
        /// Register a component directly into a component registry.
        /// </summary>
        /// <typeparam name="TLimit"></typeparam>
        /// <typeparam name="TActivatorData"></typeparam>
        /// <typeparam name="TSingleRegistrationStyle"></typeparam>
        /// <param name="cr"></param>
        /// <param name="rb"></param>
        /// <param name="activator"></param>
        public static void RegisterSingleComponent<TLimit, TActivatorData, TSingleRegistrationStyle>(
            IComponentRegistry cr,
            RegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> rb,
            IInstanceActivator activator)
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            IEnumerable<Service> services = rb.RegistrationData.Services;
            if (rb.RegistrationData.Services.Count == 0)
                services = new Service[] { new TypedService(activator.LimitType) };

            var registration = CreateRegistration(
                rb.RegistrationStyle.Id,
                rb.RegistrationData,
                activator,
                services);

            cr.Register(registration);

            var registeredEventArgs = new ComponentRegisteredEventArgs(cr, registration);
            foreach (var rh in rb.RegistrationStyle.RegisteredHandlers)
                rh(cr, registeredEventArgs);
        }

        internal static IComponentRegistration CreateRegistration(
            Guid id,
            RegistrationData data,
            IInstanceActivator activator,
            IEnumerable<Service> services)
        {
            var limitType = activator.LimitType;
            if (limitType != typeof(object))
                foreach (var ts in services.OfType<TypedService>())
                    if (!ts.ServiceType.IsAssignableFrom(limitType))
                        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                            RegistrationExtensionsResources.ComponentDoesNotSupportService, limitType, ts));

            var registration =
                new ComponentRegistration(
                    id,
                    activator,
                    data.Lifetime,
                    data.Sharing,
                    data.Ownership,
                    services,
                    data.ExtendedProperties);

            foreach (var p in data.PreparingHandlers)
                registration.Preparing += p;

            foreach (var ac in data.ActivatingHandlers)
                registration.Activating += ac;

            foreach (var ad in data.ActivatedHandlers)
                registration.Activated += ad;

            return registration;
        }

        /// <summary>
        /// Register an un-parameterised generic type, e.g. <code>Repository&lt;&gt;</code>.
        /// Concrete types will be made as they are requested, e.g. with <code>Resolve&lt;Repository&lt;int&gt;&gt;()</code>.
        /// </summary>
        /// <param name="builder">Container builder.</param>
        /// <param name="implementor">The open generic implementation type.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static RegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle>
            RegisterGeneric(this ContainerBuilder builder, Type implementor)
        {
            Enforce.ArgumentNotNull(implementor, "implementor");

            var rb = new RegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle>(
                new ReflectionActivatorData(implementor),
                new DynamicRegistrationStyle());

            builder.RegisterCallback(cr => 
                cr.AddRegistrationSource(
                    new RegistrationSource<object, ReflectionActivatorData, DynamicRegistrationStyle>(
                        rb, new OpenGenericActivatorGenerator())));

            return rb;
        }
        /// <summary>
        /// Set the policy used to find candidate constructors on the implementation type.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set policy on.</param>
        /// <param name="bindingFlags">Binding flags used when searching for constructors.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static RegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            FindConstructorsWith<TLimit, TReflectionActivatorData, TStyle>(
                this RegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
                BindingFlags bindingFlags)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            return registration.FindConstructorsWith(new BindingFlagsConstructorFinder(bindingFlags));
        }

        /// <summary>
        /// Set the policy used to find candidate constructors on the implementation type.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set policy on.</param>
        /// <param name="constructorFinder">Policy to be used when searching for constructors.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static RegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            FindConstructorsWith<TLimit, TReflectionActivatorData, TStyle>(
                this RegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
                IConstructorFinder constructorFinder)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            Enforce.ArgumentNotNull(registration, "registration");
            Enforce.ArgumentNotNull(constructorFinder, "contstructorFinder");
            registration.ActivatorData.ConstructorFinder = constructorFinder;
            return registration;
        }

        /// <summary>
        /// Set the policy used to select from available constructors on the implementation type.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set policy on.</param>
        /// <param name="signature">Constructor signature to match.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static RegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            UsingConstructor<TLimit, TReflectionActivatorData, TStyle>(
                this RegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
                params Type[] signature)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            Enforce.ArgumentNotNull(signature, "signature");
            return registration.UsingConstructor(new MatchingSignatureConstructorSelector(signature));
        }

        /// <summary>
        /// Set the policy used to select from available constructors on the implementation type.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
        /// <param name="registration">Registration to set policy on.</param>
        /// <param name="constructorSelector">Policy to be used when selecting a constructor.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static RegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            UsingConstructor<TLimit, TReflectionActivatorData, TStyle>(
                this RegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
                IConstructorSelector constructorSelector)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            Enforce.ArgumentNotNull(registration, "registration");
            Enforce.ArgumentNotNull(constructorSelector, "constructorSelector");
            registration.ActivatorData.ConstructorSelector = constructorSelector;
            return registration;
        }

        /// <summary>
        /// Configure an explicit value for a constructor parameter.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
        /// <param name="registration">Registration to set parameter on.</param>
        /// <param name="parameterName">Name of a constructor parameter on the target type.</param>
        /// <param name="parameterValue">Value of a </param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static RegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            WithParameter<TLimit, TReflectionActivatorData, TStyle>(
                this RegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
                string parameterName,
                object parameterValue)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            return registration.WithParameter(new NamedParameter(parameterName, parameterValue));
        }

        /// <summary>
        /// Configure an explicit value for a constructor parameter.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
        /// <param name="registration">Registration to set parameter on.</param>
        /// <param name="parameter">The parameter to supply to the constructor.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static RegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            WithParameter<TLimit, TReflectionActivatorData, TStyle>(
                this RegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
                Parameter parameter)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            Enforce.ArgumentNotNull(registration, "registration");
            Enforce.ArgumentNotNull(parameter, "parameter");
            registration.ActivatorData.ConfiguredParameters.Add(parameter);
            return registration;
        }
    }
}
