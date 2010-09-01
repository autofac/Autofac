using System;
using System.Linq;
using System.Web;
using Autofac.Builder;
using Autofac.Core;

namespace Autofac.Integration.Web
{
    /// <summary>
    /// Extends registration syntax for common web scenarios.
    /// </summary>
    public static class RegistrationExtensions
    {
        /// <summary>
        /// Share one instance of the component within the context of a single
        /// HTTP request.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <param name="registration">The registration to configure.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle>
            InstancePerHttpRequest<TLimit, TActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration)
        {
            if (registration == null) throw new ArgumentNullException("registration");

            return registration.InstancePerMatchingLifetimeScope(WebLifetime.Request);
        }

        /// <summary>
        /// Cache instances in the web session. This implies external ownership (disposal is not
        /// available.) All dependencies must also have external ownership.
        /// </summary>
        /// <remarks>
        /// It is strongly recommended that components cached per-session do not take dependencies on
        /// other services.
        /// </remarks>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TSingleRegistrationStyle">Registration style.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <param name="registration">The registration to configure.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static void
            CacheInSession<TLimit, TActivatorData, TSingleRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> registration)
            where TActivatorData : IConcreteActivatorData
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            if (registration == null) throw new ArgumentNullException("registration");

            var services = registration.RegistrationData.Services.ToArray();
            registration.RegistrationData.ClearServices();

            registration
                .ExternallyOwned()
                .OnRegistered(e => e.ComponentRegistry.Register(RegistrationBuilder
                    .ForDelegate((c, p) => {
                        var session = HttpContext.Current.Session;
                        object result;
                        lock (session.SyncRoot)
                        {
                            result = session[e.ComponentRegistration.Id.ToString()];
                            if (result == null)
                            {
                                result = c.ResolveComponent(e.ComponentRegistration, p);
                                session[e.ComponentRegistration.Id.ToString()] = result;
                            }
                        }
                        return result;
                    })
                    .As(services)
                    .InstancePerLifetimeScope()
                    .ExternallyOwned()
                    .CreateRegistration()));
        }
    }
}
