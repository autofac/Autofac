// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2009 Autofac Contributors
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
using System.Globalization;
using System.Linq;
using System.Reflection;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Activators.ProvidedInstance;
using Autofac.Core.Activators.Reflection;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using Autofac.Features.Collections;
using Autofac.Features.GeneratedFactories;
using Autofac.Features.OpenGenerics;
using Autofac.Util;

namespace Autofac
{
    /// <summary>
    /// Adds registration syntax to the <see cref="ContainerBuilder"/> type.
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
        /// <remarks>If no services are explicitly specified for the instance, the
        /// static type <paramref name="T"/> will be used as the default service (i.e. *not* <code>instance.GetType()</code>).</remarks>
        public static RegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle>
            RegisterInstance<T>(this ContainerBuilder builder, T instance)
            where T : class
        {
            Enforce.ArgumentNotNull(builder, "builder");
            Enforce.ArgumentNotNull(instance, "instance");

            var activator = new ProvidedInstanceActivator(instance);

            var rb = new RegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle>(
                new SimpleActivatorData(activator),
                new SingleRegistrationStyle());

            rb.SingleInstance();

            builder.RegisterCallback(cr =>
            {
                if (!(rb.RegistrationData.Lifetime is RootScopeLifetime) ||
                    rb.RegistrationData.Sharing != InstanceSharing.Shared)
                    throw new InvalidOperationException(string.Format(
                        RegistrationExtensionsResources.InstanceRegistrationsAreSingleInstanceOnly, instance));

                activator.DisposeInstance = rb.RegistrationData.Ownership == InstanceOwnership.OwnedByLifetimeScope;

                RegisterSingleComponent(cr, rb, rb.ActivatorData.Activator, typeof(T));
            });

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
                    rb.ActivatorData.ConfiguredParameters,
                    rb.ActivatorData.ConfiguredProperties)));

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
                    rb.ActivatorData.ConfiguredParameters,
                    rb.ActivatorData.ConfiguredProperties)));

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
            Register<T>(
                this ContainerBuilder builder,
                Func<IComponentContext, T> @delegate)
        {
            Enforce.ArgumentNotNull(@delegate, "delegate");
            return builder.Register<T>((c, p) => @delegate(c));
        }

        /// <summary>
        /// Register a delegate as a component.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="builder">Container builder.</param>
        /// <param name="delegate">The delegate to register.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static RegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle>
            Register<T>(
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

        internal static void RegisterSingleComponent<TLimit, TActivatorData, TSingleRegistrationStyle>(
            IComponentRegistry cr,
            RegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> rb,
            IInstanceActivator activator)
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            RegisterSingleComponent(cr, rb, activator, activator.LimitType);
        }

        internal static void RegisterSingleComponent<TLimit, TActivatorData, TSingleRegistrationStyle>(
            IComponentRegistry cr,
            RegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> rb,
            IInstanceActivator activator,
            Type defaultServiceType)
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            IEnumerable<Service> services = rb.RegistrationData.Services;
            if (rb.RegistrationData.Services.Count == 0)
                services = new Service[] { new TypedService(defaultServiceType) };

            var registration = CreateRegistration(
                rb.RegistrationStyle.Id,
                rb.RegistrationData,
                activator,
                services);

            cr.Register(registration, rb.RegistrationStyle.PreserveDefaults);

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
        /// Register an un-parameterised generic type, e.g. Repository&lt;&gt;.
        /// Concrete types will be made as they are requested, e.g. with Resolve&lt;Repository&lt;int&gt;&gt;().
        /// </summary>
        /// <param name="builder">Container builder.</param>
        /// <param name="implementor">The open generic implementation type.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static RegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle>
            RegisterGeneric(this ContainerBuilder builder, Type implementor)
        {
            Enforce.ArgumentNotNull(builder, "builder");
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
        /// Specifies that the component being registered should only be made the default for services
        /// that have not already been registered.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TSingleRegistrationStyle">Registration style.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static RegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle>
            PreserveExistingDefaults<TLimit, TActivatorData, TSingleRegistrationStyle>(
                this RegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> registration)
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            registration.RegistrationStyle.PreserveDefaults = true;
            return registration;
        }

        /// <summary>
        /// Register the types in an assembly.
        /// </summary>
        /// <param name="builder">Container builder.</param>
        /// <param name="assembly">The assembly from which to register types.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static RegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            RegisterAssemblyTypes(this ContainerBuilder builder, Assembly assembly)
        {
            Enforce.ArgumentNotNull(builder, "builder");
            Enforce.ArgumentNotNull(assembly, "assembly");

            var rb = new RegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>(
                new ScanningActivatorData(),
                new DynamicRegistrationStyle());

            builder.RegisterCallback(cr =>
            {
                var tempBuilder = new ContainerBuilder();
                foreach (var t in assembly.GetTypes()
                    .Where(t => rb.ActivatorData.Filters.All(p => p(t))))
                {
                    var activator = new ReflectionActivator(
                        t,
                        rb.ActivatorData.ConstructorFinder,
                        rb.ActivatorData.ConstructorSelector,
                        rb.ActivatorData.ConfiguredParameters,
                        rb.ActivatorData.ConfiguredProperties);

                    var services = new HashSet<Service>(rb.RegistrationData.Services);

                    if (!services.Any() && !rb.ActivatorData.ServiceMappings.Any())
                        services.Add(new TypedService(t));

                    foreach (var mapping in rb.ActivatorData.ServiceMappings)
                        foreach (var s in mapping(t))
                            services.Add(s);

                    var r = CreateRegistration(Guid.NewGuid(), rb.RegistrationData, activator, services);
                    cr.Register(r);
                }
            });

            return rb;
        }

        /// <summary>
        /// Specifies a subset of types to register from a scanned assembly.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <param name="registration">Registration to filter types from.</param>
        /// <param name="predicate">Predicate that returns true for types to register.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static RegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            Where<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this RegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Func<Type, bool> predicate)
            where TScanningActivatorData : ScanningActivatorData
        {
            registration.ActivatorData.Filters.Add(predicate);
            return registration;
        }

        /// <summary>
        /// Specifies how a type from a scanned assembly is mapped to a service.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="serviceMapping">Function mapping types to services.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static RegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            As<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this RegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Func<Type, IEnumerable<Service>> serviceMapping)
            where TScanningActivatorData : ScanningActivatorData
        {
            registration.ActivatorData.ServiceMappings.Add(serviceMapping);
            return registration;
        }

        /// <summary>
        /// Specifies how a type from a scanned assembly is mapped to a service.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="serviceMapping">Function mapping types to services.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static RegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            As<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this RegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Func<Type, Service> serviceMapping)
            where TScanningActivatorData : ScanningActivatorData
        {
            return registration.As(t => new Service[] { serviceMapping(t) });
        }

        /// <summary>
        /// Specifies how a type from a scanned assembly is mapped to a service.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="serviceMapping">Function mapping types to services.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static RegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            As<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this RegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Func<Type, Type> serviceMapping)
            where TScanningActivatorData : ScanningActivatorData
        {
            return registration.As(t => new TypedService(serviceMapping(t)));
        }

        /// <summary>
        /// Specifies how a type from a scanned assembly is mapped to a service.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="serviceMapping">Function mapping types to services.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static RegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            Named<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this RegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Func<Type, string> serviceMapping)
            where TScanningActivatorData : ScanningActivatorData
        {
            return registration.As(t => new NamedService(serviceMapping(t)));
        }

        /// <summary>
        /// Specifies that a type from a scanned assembly is registered as providing all of its
        /// implemented interfaces.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static RegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            AsImplementedInterfaces<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this RegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration)
            where TScanningActivatorData : ScanningActivatorData
        {
            return registration.As(t => t.GetInterfaces().Select(i => new TypedService(i)).Cast<Service>());
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

            // Unfortunately this could cause some false positives in rare AOP/dynamic subclassing
            // scenarios. If it becomes a problem we'll address it then.

            if (registration.ActivatorData.ImplementationType.GetConstructor(signature) == null)
                throw new ArgumentException(
                    string.Format(RegistrationExtensionsResources.NoMatchingConstructorExists, registration.ActivatorData.ImplementationType));

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
        /// <param name="parameterValue">Value to supply to the parameter.</param>
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

        /// <summary>
        /// Configure an explicit value for a property.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
        /// <param name="registration">Registration to set property on.</param>
        /// <param name="propertyName">Name of a property on the target type.</param>
        /// <param name="propertyValue">Value to supply to the property.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static RegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            WithProperty<TLimit, TReflectionActivatorData, TStyle>(
                this RegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
                string propertyName,
                object propertyValue)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            return registration.WithProperty(new NamedPropertyParameter(propertyName, propertyValue));
        }

        /// <summary>
        /// Configure an explicit value for a property.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
        /// <param name="registration">Registration to set parameter on.</param>
        /// <param name="property">The property to supply.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static RegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            WithProperty<TLimit, TReflectionActivatorData, TStyle>(
                this RegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
                Parameter property)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            Enforce.ArgumentNotNull(registration, "registration");
            Enforce.ArgumentNotNull(property, "property");
            registration.ActivatorData.ConfiguredProperties.Add(property);
            return registration;
        }
        
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
            return GeneratedFactoryRegistrationExtensions.RegisterGeneratedFactory<Delegate>(builder, delegateType, service);
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
            return GeneratedFactoryRegistrationExtensions.RegisterGeneratedFactory<TDelegate>(builder, typeof(TDelegate), service);
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
        public static RegistrationBuilder<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle>
            TypedParameterMapping<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle>(
                this RegistrationBuilder<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle> registration)
            where TGeneratedFactoryActivatorData : GeneratedFactoryActivatorData
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            registration.ActivatorData.ParameterMapping = ParameterMapping.ByType;
            return registration;
        }

        /// <summary>
        /// Provide a handler to be called when the component is registred.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TSingleRegistrationStyle">Registration style.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <param name="registration">Registration add handler to.</param>
        /// <param name="handler">The handler.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static RegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle>
            OnRegistered<TLimit, TActivatorData, TSingleRegistrationStyle>(
                this RegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> registration,
                Action<ComponentRegisteredEventArgs> handler)
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            Enforce.ArgumentNotNull(registration, "registration");
            Enforce.ArgumentNotNull(handler, "handler");

            registration.RegistrationStyle.RegisteredHandlers.Add((s, e) => handler(e));

            return registration;
        }

        /// <summary>
        /// Registers the type as a collection. If no services or names are specified, the
        /// default services will be IList&lt;T&gt;, ICollection&lt;T&gt;, and IEnumerable&lt;T&gt;        
        /// </summary>
        /// <param name="elementType">The type of the collection elements.</param>
        /// <param name="builder">Container builder.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static RegistrationBuilder<object[], SimpleActivatorData, SingleRegistrationStyle>
            RegisterCollection(this ContainerBuilder builder, Type elementType)
        {
            return CollectionRegistrationExtensions.RegisterCollection<object>(builder, elementType);
        }

        /// <summary>
        /// Registers the type as a collection. If no services or names are specified, the
        /// default services will be IList&lt;T&gt;, ICollection&lt;T&gt;, and IEnumerable&lt;T&gt;        
        /// </summary>
        /// <typeparam name="T">The type of the collection elements.</typeparam>
        /// <param name="builder">Container builder.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static RegistrationBuilder<T[], SimpleActivatorData, SingleRegistrationStyle>
            RegisterCollection<T>(this ContainerBuilder builder)
        {
            return CollectionRegistrationExtensions.RegisterCollection<T>(builder, typeof(T));
        }

        /// <summary>
        /// Include the element explicitly in a collection configured using RegisterCollection.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TSingleRegistrationStyle">Registration style.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <param name="registration">Registration to export.</param>
        /// <param name="service">The collection type to include this item in.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static RegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle>
            MemberOf<TLimit, TActivatorData, TSingleRegistrationStyle>(
                this RegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> registration,
                Service service)
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            return CollectionRegistrationExtensions.MemberOf(registration, service);
        }

        /// <summary>
        /// Include the element explicitly in a collection configured using RegisterCollection.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TSingleRegistrationStyle">Registration style.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <param name="registration">Registration to export.</param>
        /// <param name="serviceType">The collection type to include this item in.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static RegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle>
            MemberOf<TLimit, TActivatorData, TSingleRegistrationStyle>(
                this RegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> registration,
                Type serviceType)
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            return registration.MemberOf(new TypedService(serviceType));
        }
    }
}
