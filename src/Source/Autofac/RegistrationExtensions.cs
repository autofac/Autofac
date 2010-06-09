// This software is part of the Autofac IoC container
// Copyright (c) 2010 Autofac Contributors
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
using System.Reflection;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.ProvidedInstance;
using Autofac.Core.Activators.Reflection;
using Autofac.Core.Lifetime;
using Autofac.Features.LightweightAdapters;
using Autofac.Features.OpenGenerics;
using Autofac.Features.Scanning;
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
            Enforce.ArgumentNotNull(builder, "builder");
            Enforce.ArgumentNotNull(module, "module");
            builder.RegisterCallback(module.Configure);
        }

        /// <summary>
        /// Add a module to the container.
        /// </summary>
        /// <param name="builder">The builder to register the module with.</param>
        /// <typeparam name="TModule">The module to add.</typeparam>
        public static void RegisterModule<TModule>(this ContainerBuilder builder)
            where TModule : IModule, new()
        {
            builder.RegisterModule(new TModule());
        }

        /// <summary>
        /// Add a component to the container.
        /// </summary>
        /// <param name="builder">The builder to register the component with.</param>
        /// <param name="registration">The component to add.</param>
        public static void RegisterComponent(this ContainerBuilder builder, IComponentRegistration registration)
        {
            Enforce.ArgumentNotNull(builder, "builder");
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
            Enforce.ArgumentNotNull(builder, "builder");
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
        /// static type <typeparamref name="T"/> will be used as the default service (i.e. *not* <code>instance.GetType()</code>).</remarks>
        public static IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle>
            RegisterInstance<T>(this ContainerBuilder builder, T instance)
            where T : class
        {
            Enforce.ArgumentNotNull(builder, "builder");
            Enforce.ArgumentNotNull(instance, "instance");

            var activator = new ProvidedInstanceActivator(instance);

            var rb = new RegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle>(
                new TypedService(typeof(T)),
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

                RegistrationBuilder.RegisterSingleComponent(cr, rb);
            });

            return rb;
        }

        /// <summary>
        /// Register a component to be created through reflection.
        /// </summary>
        /// <typeparam name="TImplementor">The type of the component implementation.</typeparam>
        /// <param name="builder">Container builder.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TImplementor, ConcreteReflectionActivatorData, SingleRegistrationStyle>
            RegisterType<TImplementor>(this ContainerBuilder builder)
        {
            Enforce.ArgumentNotNull(builder, "builder");

            var rb = RegistrationBuilder.ForType<TImplementor>();

            builder.RegisterCallback(cr => RegistrationBuilder.RegisterSingleComponent(cr, rb));

            return rb;
        }

        /// <summary>
        /// Register a component to be created through reflection.
        /// </summary>
        /// <param name="implementationType">The type of the component implementation.</param>
        /// <param name="builder">Container builder.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>
            RegisterType(this ContainerBuilder builder, Type implementationType)
        {
            Enforce.ArgumentNotNull(builder, "builder");
            Enforce.ArgumentNotNull(implementationType, "implementationType");

            var rb = RegistrationBuilder.ForType(implementationType);

            builder.RegisterCallback(cr => RegistrationBuilder.RegisterSingleComponent(cr, rb));

            return rb;
        }

        /// <summary>
        /// Register a delegate as a component.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="builder">Container builder.</param>
        /// <param name="delegate">The delegate to register.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle>
            Register<T>(
                this ContainerBuilder builder,
                Func<IComponentContext, T> @delegate)
        {
            Enforce.ArgumentNotNull(@delegate, "delegate");
            return builder.Register((c, p) => @delegate(c));
        }

        /// <summary>
        /// Register a delegate as a component.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="builder">Container builder.</param>
        /// <param name="delegate">The delegate to register.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle>
            Register<T>(
                this ContainerBuilder builder,
                Func<IComponentContext, IEnumerable<Parameter>, T> @delegate)
        {
            Enforce.ArgumentNotNull(builder, "builder");
            Enforce.ArgumentNotNull(@delegate, "delegate");

            var rb = RegistrationBuilder.ForDelegate(@delegate);

            builder.RegisterCallback(cr => RegistrationBuilder.RegisterSingleComponent(cr, rb));

            return rb;
        }

        /// <summary>
        /// Register an un-parameterised generic type, e.g. Repository&lt;&gt;.
        /// Concrete types will be made as they are requested, e.g. with Resolve&lt;Repository&lt;int&gt;&gt;().
        /// </summary>
        /// <param name="builder">Container builder.</param>
        /// <param name="implementor">The open generic implementation type.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle>
            RegisterGeneric(this ContainerBuilder builder, Type implementor)
        {
            return OpenGenericRegistrationExtensions.RegisterGeneric(builder, implementor);
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
        public static IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle>
            PreserveExistingDefaults<TLimit, TActivatorData, TSingleRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> registration)
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            Enforce.ArgumentNotNull(registration, "registration");
            registration.RegistrationStyle.PreserveDefaults = true;
            return registration;
        }

        /// <summary>
        /// Register the types in an assembly.
        /// </summary>
        /// <param name="builder">Container builder.</param>
        /// <param name="assemblies">The assemblies from which to register types.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            RegisterAssemblyTypes(this ContainerBuilder builder, params Assembly[] assemblies)
        {
            return ScanningRegistrationExtensions.RegisterAssemblyTypes(builder, assemblies);
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
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            Where<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Func<Type, bool> predicate)
            where TScanningActivatorData : ScanningActivatorData
        {
            Enforce.ArgumentNotNull(registration, "registration");
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
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            As<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Func<Type, IEnumerable<Service>> serviceMapping)
            where TScanningActivatorData : ScanningActivatorData
        {
            Enforce.ArgumentNotNull(registration, "registration");
            registration.ActivatorData.ConfigurationActions.Add((t, rb) => rb.As(serviceMapping(t).ToArray()));
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
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            As<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Func<Type, Service> serviceMapping)
            where TScanningActivatorData : ScanningActivatorData
        {
            Enforce.ArgumentNotNull(registration, "registration");
            return registration.As(t => new [] { serviceMapping(t) });
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
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            As<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Func<Type, Type> serviceMapping)
            where TScanningActivatorData : ScanningActivatorData
        {
            Enforce.ArgumentNotNull(registration, "registration");
            return registration.As(t => new TypedService(serviceMapping(t)));
        }

        /// <summary>
        /// Specifies that a type from a scanned assembly provides its own concrete type as a service.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            AsSelf<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration)
            where TScanningActivatorData : ScanningActivatorData
        {
            Enforce.ArgumentNotNull(registration, "registration");
            return registration.As(t => t);
        }

        /// <summary>
        /// Specify how a type from a scanned assembly provides metadata.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="metadataMapping">A function mapping the type to a list of metadata items.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            WithMetadata<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Func<Type, IEnumerable<KeyValuePair<string, object>>> metadataMapping)
            where TScanningActivatorData : ScanningActivatorData
        {
            Enforce.ArgumentNotNull(registration, "registration");
            registration.ActivatorData.ConfigurationActions.Add((t, rb) => rb.WithMetadata(metadataMapping(t)));
            return registration;
        }

        /// <summary>
        /// Specify how a type from a scanned assembly provides metadata.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="metadataKey">Key of the metadata item.</param>
        /// <param name="metadataValueMapping">A function retrieving the value of the item from the component type.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            WithMetadata<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                string metadataKey,
                Func<Type, object> metadataValueMapping)
            where TScanningActivatorData : ScanningActivatorData
        {
            Enforce.ArgumentNotNull(registration, "registration");
            return registration.WithMetadata(t =>
                new[] { new KeyValuePair<string, object>(metadataKey, metadataValueMapping(t)) });
        }

        /// <summary>
        /// Specifies how a type from a scanned assembly is mapped to a named service.
        /// </summary>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <typeparam name="TService">Service type provided by the component.</typeparam>
        /// <param name="serviceNameMapping">Function mapping types to service names.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            Named<TService>(
                this IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> registration,
                Func<Type, string> serviceNameMapping)
        {
            return registration.Named(serviceNameMapping, typeof(TService));
        }

        /// <summary>
        /// Specifies how a type from a scanned assembly is mapped to a named service.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="serviceType">Service type provided by the component.</param>
        /// <param name="serviceNameMapping">Function mapping types to service names.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            Named<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Func<Type, string> serviceNameMapping,
                Type serviceType)
            where TScanningActivatorData : ScanningActivatorData
        {
            Enforce.ArgumentNotNull(registration, "registration");
            Enforce.ArgumentNotNull(serviceNameMapping, "serviceNameMapping");
            Enforce.ArgumentNotNull(serviceType, "serviceType");
            return registration.As(t => new NamedService(serviceNameMapping(t), serviceType));
        }

        /// <summary>
        /// Specifies how a type from a scanned assembly is mapped to a keyed service.
        /// </summary>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <typeparam name="TService">Service type provided by the component.</typeparam>
        /// <param name="serviceKeyMapping">Function mapping types to service keys.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            Keyed<TService>(
                this IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> registration,
                Func<Type, string> serviceKeyMapping)
        {
            return registration.Keyed(serviceKeyMapping, typeof (TService));
        }

        /// <summary>
        /// Specifies how a type from a scanned assembly is mapped to a keyed service.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="serviceType">Service type provided by the component.</param>
        /// <param name="serviceKeyMapping">Function mapping types to service keys.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            Keyed<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Func<Type, string> serviceKeyMapping,
                Type serviceType)
            where TScanningActivatorData : ScanningActivatorData
        {
            Enforce.ArgumentNotNull(registration, "registration");
            Enforce.ArgumentNotNull(serviceKeyMapping, "serviceKeyMapping");
            Enforce.ArgumentNotNull(serviceType, "serviceType");
            return registration.As(t => new KeyedService(serviceKeyMapping(t), serviceType));
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
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            AsImplementedInterfaces<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration)
            where TScanningActivatorData : ScanningActivatorData
        {
            Enforce.ArgumentNotNull(registration, "registration");
// ReSharper disable RedundantEnumerableCastCall
            return registration.As(t => t.GetInterfaces().Select(i => new TypedService(i)).Cast<Service>());
// ReSharper restore RedundantEnumerableCastCall
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
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            FindConstructorsWith<TLimit, TReflectionActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
                BindingFlags bindingFlags)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            Enforce.ArgumentNotNull(registration, "registration");
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
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            FindConstructorsWith<TLimit, TReflectionActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
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
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            UsingConstructor<TLimit, TReflectionActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
                params Type[] signature)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            Enforce.ArgumentNotNull(registration, "registration");
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
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            UsingConstructor<TLimit, TReflectionActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
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
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            WithParameter<TLimit, TReflectionActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
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
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            WithParameter<TLimit, TReflectionActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
                Parameter parameter)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            Enforce.ArgumentNotNull(registration, "registration");
            Enforce.ArgumentNotNull(parameter, "parameter");
            registration.ActivatorData.ConfiguredParameters.Add(parameter);
            return registration;
        }

        /// <summary>
        /// Configure explicit values for constructor parameters.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
        /// <param name="registration">Registration to set parameter on.</param>
        /// <param name="parameters">The parameters to supply to the constructor.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            WithParameters<TLimit, TReflectionActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
                IEnumerable<Parameter> parameters)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            Enforce.ArgumentNotNull(parameters, "parameters");

            foreach (var param in parameters)
                registration.WithParameter(param);

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
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            WithProperty<TLimit, TReflectionActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
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
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            WithProperty<TLimit, TReflectionActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
                Parameter property)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            Enforce.ArgumentNotNull(registration, "registration");
            Enforce.ArgumentNotNull(property, "property");
            registration.ActivatorData.ConfiguredProperties.Add(property);
            return registration;
        }

        /// <summary>
        /// Configure explicit values for properties.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
        /// <param name="registration">Registration to set parameter on.</param>
        /// <param name="properties">The properties to supply.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            WithProperties<TLimit, TReflectionActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
                IEnumerable<Parameter> properties)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            Enforce.ArgumentNotNull(registration, "registration");
            Enforce.ArgumentNotNull(properties, "properties");

            foreach (var prop in properties)
                registration.WithProperty(prop);

            return registration;
        }

        /// <summary>
        /// Sets the target of the registration (used for metadata generation.)
        /// </summary>
        /// <typeparam name="TLimit">The type of the limit.</typeparam>
        /// <typeparam name="TActivatorData">The type of the activator data.</typeparam>
        /// <typeparam name="TSingleRegistrationStyle">Registration style</typeparam>
        /// <param name="registration">Registration to set target for.</param>
        /// <param name="target">The target.</param>
        /// <returns>
        /// Registration builder allowing the registration to be configured.
        /// </returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle>
            Targeting<TLimit, TActivatorData, TSingleRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> registration,
                IComponentRegistration target)
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            Enforce.ArgumentNotNull(target, "target");
            registration.RegistrationStyle.Target = target;
            return registration;
        }

        /// <summary>
        /// Provide a handler to be called when the component is registered.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TSingleRegistrationStyle">Registration style.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <param name="registration">Registration add handler to.</param>
        /// <param name="handler">The handler.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle>
            OnRegistered<TLimit, TActivatorData, TSingleRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> registration,
                Action<ComponentRegisteredEventArgs> handler)
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            Enforce.ArgumentNotNull(registration, "registration");
            Enforce.ArgumentNotNull(handler, "handler");

            registration.RegistrationStyle.RegisteredHandlers.Add((s, e) => handler(e));

            return registration;
        }

        /// <summary>
        /// Provide a handler to be called when the component is registred.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration add handler to.</param>
        /// <param name="handler">The handler.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, ScanningActivatorData, TRegistrationStyle>
            OnRegistered<TLimit, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, ScanningActivatorData, TRegistrationStyle> registration,
                Action<ComponentRegisteredEventArgs> handler)
        {
            Enforce.ArgumentNotNull(registration, "registration");
            Enforce.ArgumentNotNull(handler, "handler");

            registration.ActivatorData.ConfigurationActions.Add((t, rb) => rb.OnRegistered(handler));

            return registration;
        }

        /// <summary>
        /// Specifies that a type from a scanned assembly is registered if it implements an interface
        /// that closes the provided open generic interface type.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="openGenericServiceType">The open generic interface or base class type for which implementations will be found.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            AsClosedTypesOf<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration, Type openGenericServiceType)
            where TScanningActivatorData : ScanningActivatorData
        {
            return ScanningRegistrationExtensions.AsClosedTypesOf(registration, openGenericServiceType);
        }

        /// <summary>
        /// Filters the scanned types to include only those assignable to the provided
        /// type.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <param name="registration">Registration to filter types from.</param>
        /// <param name="type">The type or interface which all classes must be assignable from.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            AssignableTo<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Type type)
            where TScanningActivatorData : ScanningActivatorData
        {
            return ScanningRegistrationExtensions.AssignableTo(registration, type);
        }

        /// <summary>
        /// Filters the scanned types to include only those assignable to the provided
        /// type.
        /// </summary>
        /// <param name="registration">Registration to filter types from.</param>
        /// <typeparam name="T">The type or interface which all classes must be assignable from.</typeparam>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            AssignableTo<T>(this IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> registration)
        {
            return registration.AssignableTo(typeof(T));
        }

        /// <summary>
        /// Filters the scanned types to exclude the provided type.
        /// </summary>
        /// <param name="registration">Registration to filter types from.</param>
        /// <typeparam name="T">The concrete type to exclude.</typeparam>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            Except<T>(this IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> registration)
        {
            return registration.Where(t => t != typeof(T));
        }

        /// <summary>
        /// Filters the scanned types to exclude the provided type, providing specific configuration for
        /// the excluded type.
        /// </summary>
        /// <param name="registration">Registration to filter types from.</param>
        /// <param name="customisedRegistration">Registration for the excepted type.</param>
        /// <typeparam name="T">The concrete type to exclude.</typeparam>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            Except<T>(
                this IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> registration,
                Action<IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle>> customisedRegistration)
        {
            var result = registration.Except<T>();

            result.ActivatorData.PostScanningCallbacks.Add(cr => {
                var rb = RegistrationBuilder.ForType<T>();
                customisedRegistration(rb);
                RegistrationBuilder.RegisterSingleComponent(cr, rb);
            });

            return result;
        }

        /// <summary>
        /// Filters the scanned types to include only those in the namespace of the provided type
        /// or one of its sub-namespaces.
        /// </summary>
        /// <param name="registration">Registration to filter types from.</param>
        /// <typeparam name="T">A type in the target namespace.</typeparam>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            InNamespaceOf<T>(this IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> registration)
        {
            return registration.InNamespace(typeof(T).Namespace);
        }

        /// <summary>
        /// Filters the scanned types to include only those in the provided namespace
        /// or one of its sub-namespaces.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <param name="registration">Registration to filter types from.</param>
        /// <param name="ns">The namespace from which types will be selected.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            InNamespace<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                string ns)
            where TScanningActivatorData : ScanningActivatorData
        {
            Enforce.ArgumentNotNullOrEmpty(ns, "ns");
            return registration.Where(t =>
                t.Namespace != null &&
                (t.Namespace == ns || t.Namespace.StartsWith(ns + ".")));
        }

        /// <summary>
        /// Adapt all components implementing service <typeparamref name="TFrom"/>
        /// to provide <typeparamref name="TTo"/> using the provided <paramref name="adapter"/>
        /// function.
        /// </summary>
        /// <typeparam name="TFrom">Service type to adapt from.</typeparam>
        /// <typeparam name="TTo">Service type to adapt to. Must not be the
        /// same as <typeparamref name="TFrom"/>.</typeparam>
        /// <param name="builder">Container builder.</param>
        /// <param name="adapter">Function adapting <typeparamref name="TFrom"/> to
        /// service <typeparamref name="TTo"/>, given the context and parameters.</param>
        public static IRegistrationBuilder<TTo, LightweightAdapterActivatorData, DynamicRegistrationStyle>
            RegisterAdapter<TFrom, TTo>(
                this ContainerBuilder builder,
                Func<IComponentContext, IEnumerable<Parameter>, TFrom, TTo> adapter)
        {
            if (builder == null) throw new ArgumentNullException("builder");
            if (adapter == null) throw new ArgumentNullException("adapter");

            return LightweightAdapterRegistrationExtensions.RegisterAdapter(builder, adapter);
        }

        /// <summary>
        /// Adapt all components implementing service <typeparamref name="TFrom"/>
        /// to provide <typeparamref name="TTo"/> using the provided <paramref name="adapter"/>
        /// function.
        /// </summary>
        /// <typeparam name="TFrom">Service type to adapt from.</typeparam>
        /// <typeparam name="TTo">Service type to adapt to. Must not be the
        /// same as <typeparamref name="TFrom"/>.</typeparam>
        /// <param name="builder">Container builder.</param>
        /// <param name="adapter">Function adapting <typeparamref name="TFrom"/> to
        /// service <typeparamref name="TTo"/>, given the context.</param>
        public static IRegistrationBuilder<TTo, LightweightAdapterActivatorData, DynamicRegistrationStyle>
            RegisterAdapter<TFrom, TTo>(
                this ContainerBuilder builder,
                Func<IComponentContext, TFrom, TTo> adapter)
        {
            if (builder == null) throw new ArgumentNullException("builder");
            if (adapter == null) throw new ArgumentNullException("adapter");

            return builder.RegisterAdapter<TFrom, TTo>((c, p, f) => adapter(c, f));
        }

        /// <summary>
        /// Adapt all components implementing service <typeparamref name="TFrom"/>
        /// to provide <typeparamref name="TTo"/> using the provided <paramref name="adapter"/>
        /// function.
        /// </summary>
        /// <typeparam name="TFrom">Service type to adapt from.</typeparam>
        /// <typeparam name="TTo">Service type to adapt to. Must not be the
        /// same as <typeparamref name="TFrom"/>.</typeparam>
        /// <param name="builder">Container builder.</param>
        /// <param name="adapter">Function adapting <typeparamref name="TFrom"/> to
        /// service <typeparamref name="TTo"/>.</param>
        public static IRegistrationBuilder<TTo, LightweightAdapterActivatorData, DynamicRegistrationStyle>
            RegisterAdapter<TFrom, TTo>(
                this ContainerBuilder builder,
                Func<TFrom, TTo> adapter)
        {
            if (builder == null) throw new ArgumentNullException("builder");
            if (adapter == null) throw new ArgumentNullException("adapter");

            return builder.RegisterAdapter<TFrom, TTo>((c, p, f) => adapter(f));
        }
    }
}
