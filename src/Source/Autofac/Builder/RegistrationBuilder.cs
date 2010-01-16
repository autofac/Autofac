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
    /// Static factory methods to simplify the creation and handling of RegistrationBuilder{L,A,R}.
    /// </summary>
    /// <example>
    /// To create an <see cref="IComponentRegistration"/> for a specific type, use:
    /// <code>
    /// var rb = RegistrationBuilder.ForType(t).Named("foo").ExternallyOwned();
    /// var cr = RegistrationBuilder.CreateRegistration(rb);
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
        public static RegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle> ForDelegate<T>(Func<IComponentContext, IEnumerable<Parameter>, T> @delegate)
        {
            return new RegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle>(
                new SimpleActivatorData(new DelegateActivator(typeof(T), (c, p) => @delegate(c, p))),
                new SingleRegistrationStyle(typeof(T)));
        }

        /// <summary>
        /// Creates a registration builder for the provided type.
        /// </summary>
        /// <typeparam name="TImplementor">Implementation type to register.</typeparam>
        /// <returns>A registration builder.</returns>
        public static RegistrationBuilder<TImplementor, ConcreteReflectionActivatorData, SingleRegistrationStyle> ForType<TImplementor>()
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
        public static RegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> ForType(Type implementationType)
        {
            return new RegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>(
                new ConcreteReflectionActivatorData(implementationType),
                new SingleRegistrationStyle(implementationType));
        }

        /// <summary>
        /// Create an IComponentRegistration from a RegistrationBuilder.
        /// </summary>
        /// <typeparam name="TLimit"></typeparam>
        /// <typeparam name="TActivatorData"></typeparam>
        /// <typeparam name="TSingleRegistrationStyle"></typeparam>
        /// <param name="rb">The registration builder.</param>
        /// <returns>An IComponentRegistration.</returns>
        public static IComponentRegistration CreateRegistration<TLimit, TActivatorData, TSingleRegistrationStyle>(
            RegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> rb)
            where TSingleRegistrationStyle : SingleRegistrationStyle
            where TActivatorData : IConcreteActivatorData
        {
            IEnumerable<Service> services = rb.RegistrationData.Services;
            if (rb.RegistrationData.Services.Count == 0)
                services = new Service[] { new TypedService(rb.RegistrationStyle.DefaultServiceType) };

            return CreateRegistration(
                rb.RegistrationStyle.Id,
                rb.RegistrationData,
                rb.ActivatorData.Activator,
                services);
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
            var limitType = activator.LimitType;
            if (limitType != typeof(object))
                foreach (var ts in services.OfType<TypedService>())
                    if (!ts.ServiceType.IsAssignableFrom(limitType))
                        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                            RegistrationBuilderResources.ComponentDoesNotSupportService, limitType, ts));

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
            RegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> rb)
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
