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
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.Scanning;

namespace Autofac.Integration.Mvc
{
    /// <summary>
    /// Extends <see cref="ContainerBuilder"/> with methods to support ASP.NET MVC.
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

            return registration.InstancePerMatchingLifetimeScope(RequestLifetimeScopeProvider.HttpRequestTag);
        }

        /// <summary>
        /// Register types that implement IController in the provided assemblies.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="controllerAssemblies">Assemblies to scan for controllers.</param>
        /// <returns>Registration builder allowing the controller components to be customised.</returns>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            RegisterControllers(
                this ContainerBuilder builder,
                params Assembly[] controllerAssemblies)
        {
            return builder.RegisterAssemblyTypes(controllerAssemblies)
                .Where(t => typeof(IController).IsAssignableFrom(t) &&
                    t.Name.EndsWith("Controller"));
        }

        /// <summary>
        /// Inject an IActionInvoker into the controller's ActionInvoker property.
        /// </summary>
        /// <typeparam name="TLimit">Limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registrationBuilder">The registration builder.</param>
        /// <returns>A registration builder.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle>
            InjectActionInvoker<TLimit, TActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registrationBuilder)
        {
            return registrationBuilder.InjectActionInvoker(new TypedService(typeof(IActionInvoker)));
        }

        /// <summary>
        /// Inject an IActionInvoker into the controller's ActionInvoker property.
        /// </summary>
        /// <typeparam name="TLimit">Limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registrationBuilder">The registration builder.</param>
        /// <param name="actionInvokerService">Service used to resolve the action invoker.</param>
        /// <returns>A registration builder.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle>
            InjectActionInvoker<TLimit, TActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registrationBuilder,
                Service actionInvokerService)
        {
            if (registrationBuilder == null) throw new ArgumentNullException("registrationBuilder");
            if (actionInvokerService == null) throw new ArgumentNullException("actionInvokerService");

            return registrationBuilder.OnActivating(e =>
            {
                var controller = e.Instance as Controller;
                if (controller != null)
                    controller.ActionInvoker = (IActionInvoker)e.Context.ResolveService(actionInvokerService);
            });
        }

        /// <summary>
        /// Registers the <see cref="AutofacModelBinderProvider"/>.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        public static void RegisterModelBinderProvider(this ContainerBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException("builder");

            builder.RegisterType<AutofacModelBinderProvider>()
                .As<IModelBinderProvider>()
                .SingleInstance();
        }

        /// <summary>
        /// Register types that implement <see cref="IModelBinder"/> in the provided assemblies.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="modelBinderAssemblies">Assemblies to scan for model binders.</param>
        /// <returns>A registration builder.</returns>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            RegisterModelBinders(this ContainerBuilder builder, params Assembly[] modelBinderAssemblies)
        {
            if (builder == null) throw new ArgumentNullException("builder");
            if (modelBinderAssemblies == null) throw new ArgumentNullException("modelBinderAssemblies");

            return builder.RegisterAssemblyTypes(modelBinderAssemblies)
                .Where(type => typeof(IModelBinder).IsAssignableFrom(type))
                .As<IModelBinder>()
                .InstancePerHttpRequest()
                .WithMetadata(AutofacModelBinderProvider.MetadataKey, type => 
                    (from ModelBinderTypeAttribute attribute in type.GetCustomAttributes(typeof(ModelBinderTypeAttribute), true)
                     from targetType in attribute.TargetTypes
                    select targetType).ToList());
        }

        /// <summary>
        /// Registers the <see cref="AutofacFilterAttributeFilterProvider"/>.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        public static void RegisterFilterProvider(this ContainerBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException("builder");

            foreach (var provider in FilterProviders.Providers.OfType<FilterAttributeFilterProvider>().ToArray())
                FilterProviders.Providers.Remove(provider);

            builder.RegisterType<AutofacFilterAttributeFilterProvider>()
                .As<IFilterProvider>()
                .SingleInstance();
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
                    .ForDelegate((c, p) =>
                    {
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
