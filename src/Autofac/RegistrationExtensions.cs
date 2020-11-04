// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
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

namespace Autofac
{
    /// <summary>
    /// Adds registration syntax to the <see cref="ContainerBuilder"/> type.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public static partial class RegistrationExtensions
    {
        /// <summary>
        /// Add a component to the container.
        /// </summary>
        /// <param name="builder">The builder to register the component with.</param>
        /// <param name="registration">The component to add.</param>
        public static void RegisterComponent(this ContainerBuilder builder, IComponentRegistration registration)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            builder.RegisterCallback(cr => cr.Register(registration));
        }

        /// <summary>
        /// Register an instance as a component.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="builder">Container builder.</param>
        /// <param name="instance">The instance to register.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        /// <remarks>If no services are explicitly specified for the instance, the
        /// static type <typeparamref name="T"/> will be used as the default service (i.e. *not* <c>instance.GetType()</c>).</remarks>
        public static IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle>
            RegisterInstance<T>(this ContainerBuilder builder, T instance)
            where T : class
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var activator = new ProvidedInstanceActivator(instance);

            var rb = new RegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle>(
                new TypedService(typeof(T)),
                new SimpleActivatorData(activator),
                new SingleRegistrationStyle());

            rb.SingleInstance();

            rb.RegistrationData.DeferredCallback = builder.RegisterCallback(cr =>
            {
                if (!(rb.RegistrationData.Lifetime is RootScopeLifetime) ||
                    rb.RegistrationData.Sharing != InstanceSharing.Shared)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, RegistrationExtensionsResources.InstanceRegistrationsAreSingleInstanceOnly, instance));
                }

                activator.DisposeInstance = rb.RegistrationData.Ownership == InstanceOwnership.OwnedByLifetimeScope;

                // https://github.com/autofac/Autofac/issues/1102
                // Single instance registrations with any custom activation phases (i.e. activation handlers) need to be auto-activated,
                // so that other behaviour (such as OnRelease) that expects 'normal' object lifetime behaviour works as expected.
                if (rb.ResolvePipeline.Middleware.Any(s => s.Phase == PipelinePhase.Activation))
                {
                    var autoStartService = rb.RegistrationData.Services.First();

                    var activationRegistration = new RegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle>(
                        new AutoActivateService(),
                        new SimpleActivatorData(new DelegateActivator(typeof(T), (c, p) => c.ResolveService(autoStartService))),
                        new SingleRegistrationStyle());

                    RegistrationBuilder.RegisterSingleComponent(cr, activationRegistration);
                }

                RegistrationBuilder.RegisterSingleComponent(cr, rb);
            });

            return rb;
        }

        /// <summary>
        /// Register a component to be created through reflection.
        /// </summary>
        /// <typeparam name="TImplementer">The type of the component implementation.</typeparam>
        /// <param name="builder">Container builder.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TImplementer, ConcreteReflectionActivatorData, SingleRegistrationStyle>
            RegisterType<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementer>(this ContainerBuilder builder)
            where TImplementer : notnull
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var rb = RegistrationBuilder.ForType<TImplementer>();

            rb.RegistrationData.DeferredCallback = builder.RegisterCallback(cr => RegistrationBuilder.RegisterSingleComponent(cr, rb));

            return rb;
        }

        /// <summary>
        /// Register a component to be created through reflection.
        /// </summary>
        /// <param name="implementationType">The type of the component implementation.</param>
        /// <param name="builder">Container builder.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>
            RegisterType(this ContainerBuilder builder, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (implementationType == null)
            {
                throw new ArgumentNullException(nameof(implementationType));
            }

            var rb = RegistrationBuilder.ForType(implementationType);

            rb.RegistrationData.DeferredCallback = builder.RegisterCallback(cr => RegistrationBuilder.RegisterSingleComponent(cr, rb));

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
            where T : notnull
        {
            if (@delegate == null)
            {
                throw new ArgumentNullException(nameof(@delegate));
            }

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
            where T : notnull
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (@delegate == null)
            {
                throw new ArgumentNullException(nameof(@delegate));
            }

            var rb = RegistrationBuilder.ForDelegate(@delegate);

            rb.RegistrationData.DeferredCallback = builder.RegisterCallback(cr => RegistrationBuilder.RegisterSingleComponent(cr, rb));

            return rb;
        }

        /// <summary>
        /// Specifies that the component being registered should only be made the default for services
        /// that have not already been registered.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TSingleRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle>
            PreserveExistingDefaults<TLimit, TActivatorData, TSingleRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> registration)
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            registration.RegistrationStyle.PreserveDefaults = true;
            return registration;
        }

        /// <summary>
        /// Specify how a type from a scanned assembly provides metadata.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set metadata on.</param>
        /// <param name="metadataMapping">A function mapping the type to a list of metadata items.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            WithMetadata<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Func<Type, IEnumerable<KeyValuePair<string, object?>>> metadataMapping)
            where TScanningActivatorData : ScanningActivatorData
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            registration.ActivatorData.ConfigurationActions.Add((t, rb) => rb.WithMetadata(metadataMapping(t)));
            return registration;
        }

        /// <summary>
        /// Use the properties of an attribute (or interface implemented by an attribute) on the scanned type
        /// to provide metadata values.
        /// </summary>
        /// <remarks>Inherited attributes are supported; however, there must be at most one matching attribute
        /// in the inheritance chain.</remarks>
        /// <typeparam name="TAttribute">The attribute applied to the scanned type.</typeparam>
        /// <param name="registration">Registration to set metadata on.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            WithMetadataFrom<TAttribute>(
                this IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> registration)
        {
            var attrType = typeof(TAttribute);
            var metadataProperties = attrType
                .GetRuntimeProperties()
                .Where(pi => pi.CanRead);

            return registration.WithMetadata(t =>
            {
                var attrs = t.GetCustomAttributes(true).OfType<TAttribute>().ToList();

                if (attrs.Count == 0)
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, RegistrationExtensionsResources.MetadataAttributeNotFound, typeof(TAttribute), t));
                }

                if (attrs.Count != 1)
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, RegistrationExtensionsResources.MultipleMetadataAttributesSameType, typeof(TAttribute), t));
                }

                var attr = attrs[0];
                return metadataProperties.Select(p => new KeyValuePair<string, object?>(p.Name, p.GetValue(attr, null)));
            });
        }

        /// <summary>
        /// Specify how a type from a scanned assembly provides metadata.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
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
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            return registration.WithMetadata(t =>
                new[] { new KeyValuePair<string, object?>(metadataKey, metadataValueMapping(t)) });
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
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            registration.ActivatorData.ConstructorFinder = constructorFinder ?? throw new ArgumentNullException(nameof(constructorFinder));
            return registration;
        }

        /// <summary>
        /// Set the policy used to find candidate constructors on the implementation type.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set policy on.</param>
        /// <param name="finder">A function that returns the constructors to select from.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            FindConstructorsWith<TLimit, TReflectionActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
                Func<Type, ConstructorInfo[]> finder)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            return registration.FindConstructorsWith(new DefaultConstructorFinder(finder));
        }

        /// <summary>
        /// Configure the component so that any properties whose types are registered in the
        /// container will be wired to instances of the appropriate service.
        /// </summary>
        /// <param name="registration">Registration to auto-wire properties.</param>
        /// <param name="wiringFlags">Set wiring options such as circular dependency wiring support.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle>
            PropertiesAutowired<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TLimit, TActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registration, PropertyWiringOptions wiringFlags = PropertyWiringOptions.None)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            var preserveSetValues = (int)(wiringFlags & PropertyWiringOptions.PreserveSetValues) != 0;
            var allowCircularDependencies = (int)(wiringFlags & PropertyWiringOptions.AllowCircularDependencies) != 0;

            return registration.PropertiesAutowired(new DefaultPropertySelector(preserveSetValues), allowCircularDependencies);
        }

        /// <summary>
        /// Set the policy used to find candidate properties on the implementation type.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set policy on.</param>
        /// <param name="propertySelector">Policy to be used when searching for properties to inject.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        [RequiresUnreferencedCode("Autowired properties with a custom selector may not be compatible with member-level trimming.")]
        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle> PropertiesAutowired<TLimit, TActivatorData, TStyle>(
            this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration,
            Func<PropertyInfo, object, bool> propertySelector)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            return registration.PropertiesAutowired(new DelegatePropertySelector(propertySelector));
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
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            if (signature == null)
            {
                throw new ArgumentNullException(nameof(signature));
            }

            // Unfortunately this could cause some false positives in rare AOP/dynamic subclassing
            // scenarios. If it becomes a problem we'll address it then.
            if (registration.ActivatorData.ImplementationType.GetMatchingConstructor(signature) == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, RegistrationExtensionsResources.NoMatchingConstructorExists, registration.ActivatorData.ImplementationType));
            }

            return registration.UsingConstructor(new MatchingSignatureConstructorSelector(signature));
        }

        /// <summary>
        /// Set the policy used to select from available constructors on the implementation type.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set policy on.</param>
        /// <param name="constructorSelector">Policy to be used when selecting a constructor.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            UsingConstructor<TLimit, TReflectionActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
                IConstructorSelector constructorSelector)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            registration.ActivatorData.ConstructorSelector = constructorSelector ?? throw new ArgumentNullException(nameof(constructorSelector));
            return registration;
        }

        /// <summary>
        /// Set the policy used to select from available constructors on the implementation type.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set policy on.</param>
        /// <param name="constructorSelector">Expression demonstrating how the constructor is called.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            UsingConstructor<TLimit, TReflectionActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
                Expression<Func<TLimit>> constructorSelector)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            if (constructorSelector == null)
            {
                throw new ArgumentNullException(nameof(constructorSelector));
            }

            var constructor = ReflectionExtensions.GetConstructor(constructorSelector);
            var parameterTypes = constructor.GetParameters().Select(p => p.ParameterType).ToArray();
            return UsingConstructor(registration, parameterTypes);
        }

        /// <summary>
        /// Configure an explicit value for a constructor parameter.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set parameter on.</param>
        /// <param name="parameterName">Name of a constructor parameter on the target type.</param>
        /// <param name="parameterValue">Value to supply to the parameter.</param>0
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
        /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set parameter on.</param>
        /// <param name="parameter">The parameter to supply to the constructor.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            WithParameter<TLimit, TReflectionActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
                Parameter parameter)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            registration.ActivatorData.ConfiguredParameters.Add(parameter);
            return registration;
        }

        /// <summary>
        /// Configure an explicit value for a constructor parameter.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set parameter on.</param>
        /// <param name="parameterSelector">A predicate selecting the parameter to set.</param>
        /// <param name="valueProvider">The provider that will generate the parameter value.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            WithParameter<TLimit, TReflectionActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
                Func<ParameterInfo, IComponentContext, bool> parameterSelector,
                Func<ParameterInfo, IComponentContext, object?> valueProvider)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            if (parameterSelector == null)
            {
                throw new ArgumentNullException(nameof(parameterSelector));
            }

            if (valueProvider == null)
            {
                throw new ArgumentNullException(nameof(valueProvider));
            }

            return registration.WithParameter(
                new ResolvedParameter(parameterSelector, valueProvider));
        }

        /// <summary>
        /// Configure explicit values for constructor parameters.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set parameter on.</param>
        /// <param name="parameters">The parameters to supply to the constructor.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            WithParameters<TLimit, TReflectionActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
                IEnumerable<Parameter> parameters)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            foreach (var param in parameters)
            {
                registration.WithParameter(param);
            }

            return registration;
        }

        /// <summary>
        /// Configure an explicit value for a property.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
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
        /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set parameter on.</param>
        /// <param name="property">The property to supply.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            WithProperty<TLimit, TReflectionActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
                Parameter property)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            registration.ActivatorData.ConfiguredProperties.Add(property);
            return registration;
        }

        /// <summary>
        /// Configure an explicit value for a property.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <typeparam name="TProperty">Type of the property being assigned.</typeparam>
        /// <param name="registration">Registration to set property on.</param>
        /// <param name="propertyExpression">Expression of a property on the target type.</param>
        /// <param name="propertyValue">Value to supply to the property.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            WithProperty<TLimit, TReflectionActivatorData, TStyle, TProperty>(
                this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
                Expression<Func<TLimit, TProperty>> propertyExpression,
                TProperty propertyValue)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            if (propertyExpression == null)
            {
                throw new ArgumentNullException(nameof(propertyExpression));
            }

            if (propertyValue == null)
            {
                throw new ArgumentNullException(nameof(propertyValue));
            }

            var propertyInfo = (propertyExpression.Body as MemberExpression)?.Member as PropertyInfo;
            if (propertyInfo == null)
            {
                throw new ArgumentOutOfRangeException(nameof(propertyExpression), RegistrationExtensionsResources.ExpressionDoesNotReferToProperty);
            }

            return registration.WithProperty(new NamedPropertyParameter(propertyInfo.Name, propertyValue));
        }

        /// <summary>
        /// Configure explicit values for properties.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set parameter on.</param>
        /// <param name="properties">The properties to supply.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            WithProperties<TLimit, TReflectionActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
                IEnumerable<Parameter> properties)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            foreach (var prop in properties)
            {
                registration.WithProperty(prop);
            }

            return registration;
        }

        /// <summary>
        /// Sets the target of the registration (used for metadata generation).
        /// </summary>
        /// <typeparam name="TLimit">The type of the limit.</typeparam>
        /// <typeparam name="TActivatorData">The type of the activator data.</typeparam>
        /// <typeparam name="TSingleRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set target for.</param>
        /// <param name="target">The target.</param>
        /// <returns>
        /// Registration builder allowing the registration to be configured.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="registration" /> or <paramref name="target" /> is <see langword="null" />.
        /// </exception>
        public static IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle>
            Targeting<TLimit, TActivatorData, TSingleRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> registration,
                IComponentRegistration target)
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            registration.RegistrationStyle.Target = target.Target;

            return registration;
        }

        /// <summary>
        /// Wraps a registration in an implicit <see cref="Autofac.IStartable"/> and automatically
        /// activates the registration after the container is built.
        /// </summary>
        /// <param name="registration">Registration to set release action for.</param>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        /// <remarks>
        /// <para>
        /// While you can implement an <see cref="Autofac.IStartable"/> to perform some logic at
        /// container build time, sometimes you need to just activate a registered component and
        /// that's it. This extension allows you to automatically activate a registration on
        /// container build. No additional logic is executed and the resolved instance is not held
        /// so container disposal will end up disposing of the instance.
        /// </para>
        /// <para>
        /// Depending on how you register the lifetime of the component, you may get an exception
        /// when you build the container - components that are scoped to specific lifetimes (like
        /// ASP.NET components scoped to a request lifetime) will fail to resolve because the
        /// appropriate lifetime is not available.
        /// </para>
        /// </remarks>
        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle>
            AutoActivate<TLimit, TActivatorData, TRegistrationStyle>(
            this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registration)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            registration.RegistrationData.AddService(new AutoActivateService());
            return registration;
        }

        /// <summary>
        /// Share one instance of the component within the context of a single
        /// web/HTTP/API request. Only available for integration that supports
        /// per-request dependencies (e.g., MVC, Web API, web forms, etc.).
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">The registration to configure.</param>
        /// <param name="lifetimeScopeTags">Additional tags applied for matching lifetime scopes.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="registration" /> is <see langword="null" />.
        /// </exception>
        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle>
            InstancePerRequest<TLimit, TActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration, params object[] lifetimeScopeTags)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            var tags = new[] { MatchingScopeLifetimeTags.RequestLifetimeScopeTag }.Concat(lifetimeScopeTags).ToArray();
            return registration.InstancePerMatchingLifetimeScope(tags);
        }
    }
}
