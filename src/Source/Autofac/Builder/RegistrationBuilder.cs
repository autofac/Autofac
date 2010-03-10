using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autofac.Core;
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Registration;

namespace Autofac.Builder
{
    /// <summary>
    /// Static factory methods to simplify the creation and handling of IRegistrationBuilder{L,A,R}.
    /// </summary>
    /// <example>
    /// To create an <see cref="IComponentRegistration"/> for a specific type, use:
    /// <code>
    /// var rb = IRegistrationBuilder.ForType(t).Named("foo").ExternallyOwned();
    /// var cr = IRegistrationBuilder.CreateRegistration(rb);
    /// </code>
    /// </example>
    public static class RegistrationBuilder
    {
        /// <summary>
        /// Creates a registration builder for the provided delegate.
        /// </summary>
        /// <typeparam name="T">Instance type returned by delegate.</typeparam>
        /// <param name="delegate">Delegate to register.</param>
        /// <returns>A registration builder.</returns>
        public static IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle> ForDelegate<T>(Func<IComponentContext, IEnumerable<Parameter>, T> @delegate)
        {
            return new RegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle>(
                new SimpleActivatorData(new DelegateActivator(typeof(T), (c, p) => @delegate(c, p))),
                new SingleRegistrationStyle(typeof(T)));
        }

        /// <summary>
        /// Creates a registration builder for the provided delegate.
        /// </summary>
        /// <param name="delegate">Delegate to register.</param>
        /// <param name="limitType">Most specific type return value of delegate can be cast to.</param>
        /// <returns>A registration builder.</returns>
        public static IRegistrationBuilder<object, SimpleActivatorData, SingleRegistrationStyle> ForDelegate(Type limitType, Func<IComponentContext, IEnumerable<Parameter>, object> @delegate)
        {
            return new RegistrationBuilder<object, SimpleActivatorData, SingleRegistrationStyle>(
                new SimpleActivatorData(new DelegateActivator(limitType, @delegate)),
                new SingleRegistrationStyle(limitType));
        }

        /// <summary>
        /// Creates a registration builder for the provided type.
        /// </summary>
        /// <typeparam name="TImplementor">Implementation type to register.</typeparam>
        /// <returns>A registration builder.</returns>
        public static IRegistrationBuilder<TImplementor, ConcreteReflectionActivatorData, SingleRegistrationStyle> ForType<TImplementor>()
        {
            return new RegistrationBuilder<TImplementor, ConcreteReflectionActivatorData, SingleRegistrationStyle>(
                new ConcreteReflectionActivatorData(typeof(TImplementor)),
                new SingleRegistrationStyle(typeof(TImplementor)));
        }

        /// <summary>
        /// Creates a registration builder for the provided type.
        /// </summary>
        /// <param name="implementationType">Implementation type to register.</param>
        /// <returns>A registration builder.</returns>
        public static IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> ForType(Type implementationType)
        {
            return new RegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>(
                new ConcreteReflectionActivatorData(implementationType),
                new SingleRegistrationStyle(implementationType));
        }

        /// <summary>
        /// Create an <see cref='IComponentRegistration'/> from a <see cref='RegistrationBuilder'/>.
        /// (There is no need to call
        /// this method when registering components through a <see cref="ContainerBuilder"/>.)
        /// </summary>
        /// <remarks>
        /// When called on the result of one of the <see cref='ContainerBuilder'/> methods,
        /// the returned registration will be different from the one the builder itself registers
        /// in the container.
        /// </remarks>
        /// <example>
        /// <code>
        /// var registration = IRegistrationBuilder.ForType&lt;Foo&gt;().CreateRegistration();
        /// </code>
        /// </example>
        /// <typeparam name="TLimit"></typeparam>
        /// <typeparam name="TActivatorData"></typeparam>
        /// <typeparam name="TSingleRegistrationStyle"></typeparam>
        /// <param name="rb">The registration builder.</param>
        /// <returns>An IComponentRegistration.</returns>
        public static IComponentRegistration CreateRegistration<TLimit, TActivatorData, TSingleRegistrationStyle>(
            this IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> rb)
            where TSingleRegistrationStyle : SingleRegistrationStyle
            where TActivatorData : IConcreteActivatorData
        {
            IEnumerable<Service> services = rb.RegistrationData.Services;
            if (rb.RegistrationData.Services.Count == 0 && !rb.RegistrationData.DefaultServiceOverridden)
                services = new Service[] { new TypedService(rb.RegistrationStyle.DefaultServiceType) };

            return CreateRegistration(
                rb.RegistrationStyle.Id,
                rb.RegistrationData,
                rb.ActivatorData.Activator,
                services,
                rb.RegistrationStyle.Target);
        }

        /// <summary>
        /// Create an IComponentRegistration from data.
        /// </summary>
        /// <param name="id">Id of the registration.</param>
        /// <param name="data">Registration data.</param>
        /// <param name="activator">Activator.</param>
        /// <param name="services">Services provided by the registration.</param>
        /// <returns>An IComponentRegistration.</returns>
        public static IComponentRegistration CreateRegistration(
            Guid id,
            RegistrationData data,
            IInstanceActivator activator,
            IEnumerable<Service> services)
        {
            return CreateRegistration(id, data, activator, services, null);
        }

        /// <summary>
        /// Create an IComponentRegistration from data.
        /// </summary>
        /// <param name="id">Id of the registration.</param>
        /// <param name="data">Registration data.</param>
        /// <param name="activator">Activator.</param>
        /// <param name="services">Services provided by the registration.</param>
        /// <param name="target">Optional; target registration.</param>
        /// <returns>An IComponentRegistration.</returns>
        public static IComponentRegistration CreateRegistration(
            Guid id,
            RegistrationData data,
            IInstanceActivator activator,
            IEnumerable<Service> services,
            IComponentRegistration target)
        {
            var limitType = activator.LimitType;
            if (limitType != typeof(object))
                foreach (var ts in services.OfType<TypedService>())
                    if (!ts.ServiceType.IsAssignableFrom(limitType))
                        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                            RegistrationBuilderResources.ComponentDoesNotSupportService, limitType, ts));

            IComponentRegistration registration;
            if (target == null)
                registration = new ComponentRegistration(
                    id,
                    activator,
                    data.Lifetime,
                    data.Sharing,
                    data.Ownership,
                    services,
                    data.Metadata);
            else
                registration = new ComponentRegistration(
                    id,
                    activator,
                    data.Lifetime,
                    data.Sharing,
                    data.Ownership,
                    services,
                    data.Metadata,
                    target);

            foreach (var p in data.PreparingHandlers)
                registration.Preparing += p;

            foreach (var ac in data.ActivatingHandlers)
                registration.Activating += ac;

            foreach (var ad in data.ActivatedHandlers)
                registration.Activated += ad;

            return registration;
        }

        /// <summary>
        /// Register a component in the component registry. This helper method is necessary
        /// in order to execute OnRegistered hooks and respect PreserveDefaults. 
        /// </summary>
        /// <remarks>Hoping to refactor this out.</remarks>
        /// <typeparam name="TLimit"></typeparam>
        /// <typeparam name="TActivatorData"></typeparam>
        /// <typeparam name="TSingleRegistrationStyle"></typeparam>
        /// <param name="cr">Component registry to make registration in.</param>
        /// <param name="rb">Registration builder with data for new registration.</param>
        public static void RegisterSingleComponent<TLimit, TActivatorData, TSingleRegistrationStyle>(
            IComponentRegistry cr,
            IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> rb)
            where TSingleRegistrationStyle : SingleRegistrationStyle
            where TActivatorData : IConcreteActivatorData
        {
            var registration = CreateRegistration(rb);

            cr.Register(registration, rb.RegistrationStyle.PreserveDefaults);

            var registeredEventArgs = new ComponentRegisteredEventArgs(cr, registration);
            foreach (var rh in rb.RegistrationStyle.RegisteredHandlers)
                rh(cr, registeredEventArgs);
        }
    }
}
