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
using System.Linq;
using System.Web;
using Autofac.Builder;

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
        public static IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle>
            CacheInSession<TLimit, TActivatorData, TSingleRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> registration)
            where TActivatorData : IConcreteActivatorData
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            if (registration == null) throw new ArgumentNullException("registration");

            var services = registration.RegistrationData.Services.ToArray();
            registration.RegistrationData.ClearServices();

            return registration
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
