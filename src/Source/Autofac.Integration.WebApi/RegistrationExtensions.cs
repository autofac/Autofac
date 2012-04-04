// This software is part of the Autofac IoC container
// Copyright (c) 2012 Autofac Contributors
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
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.ModelBinding;
using Autofac.Builder;
using Autofac.Features.Scanning;

namespace Autofac.Integration.WebApi
{
    public static class RegistrationExtensions
    {
        /// <summary>
        /// Configures the container for integration with ASP.NET Web API.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="configuration">The configuration for the <see cref="HttpServer"/> instance.</param>
        public static void ConfigureWebApi(this ContainerBuilder builder, HttpConfiguration configuration)
        {
            builder.RegisterInstance(configuration);
            builder.Register<IHttpControllerActivator>(c => new AutofacControllerActivator())
                .SingleInstance();
            builder.Register<IHttpControllerFactory>(c => new AutofacControllerFactory(c.Resolve<HttpConfiguration>(), c.Resolve<ILifetimeScope>()))
                .SingleInstance();
        }

        /// <summary>
        /// Register types that implement <see cref="IHttpController"/> in the provided assemblies.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="controllerAssemblies">Assemblies to scan for controllers.</param>
        /// <returns>Registration builder allowing the controller components to be customised.</returns>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            RegisterApiControllers(this ContainerBuilder builder, params Assembly[] controllerAssemblies)
        {
            return builder.RegisterAssemblyTypes(controllerAssemblies)
                .Where(t => typeof(IHttpController).IsAssignableFrom(t) && t.Name.EndsWith("Controller"));
        }

        /// <summary>
        /// Share one instance of the component within the context of a 
        /// single <see cref="ApiController"/> request.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <param name="registration">The registration to configure.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle>
            InstancePerApiRequest<TLimit, TActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration)
        {
            if (registration == null) throw new ArgumentNullException("registration");

            return registration.InstancePerMatchingLifetimeScope(AutofacControllerFactory.ApiRequestTag);
        }

        /// <summary>
        /// Registers the <see cref="AutofacModelBinderProvider"/>.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        public static void RegisterModelBinderProvider(this ContainerBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException("builder");

            builder.RegisterType<AutofacModelBinderProvider>()
                .As<ModelBinderProvider>()
                .SingleInstance();
        }

        /// <summary>
        /// Sets a provided registration to act as an <see cref="IModelBinder"/>
        /// for the specified list of types.
        /// </summary>
        /// <param name="registration">
        /// The registration for the type or object instance that will act as
        /// the model binder.
        /// </param>
        /// <param name="types">
        /// The list of model <see cref="System.Type"/> for which the <paramref name="registration" />
        /// should be a model binder.
        /// </param>
        /// <typeparam name="TLimit">
        /// Registration limit type.
        /// </typeparam>
        /// <typeparam name="TActivatorData">
        /// Activator data type.
        /// </typeparam>
        /// <typeparam name="TRegistrationStyle">
        /// Registration style.
        /// </typeparam>
        /// <returns>
        /// An Autofac registration that can be modified as needed.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="registration" /> or <paramref name="types" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="types" /> is empty or contains all <see langword="null" />
        /// values.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The declarative mechanism of registering model binders with Autofac
        /// is through use of <see cref="Autofac.Integration.Mvc.RegistrationExtensions.RegisterModelBinders"/>
        /// and the <see cref="Autofac.Integration.Mvc.ModelBinderTypeAttribute"/>.
        /// This method is an imperative alternative.
        /// </para>
        /// <para>
        /// The two mechanisms are mutually exclusive. If you register a model
        /// binder using <see cref="Autofac.Integration.Mvc.RegistrationExtensions.RegisterModelBinders"/>
        /// and register the same model binder with this method, the results
        /// are not automatically merged together - standard dependency
        /// registration/resolution rules will be used to manage the conflict.
        /// </para>
        /// <para>
        /// Any <see langword="null" /> values provided in <paramref name="types" />
        /// will be removed prior to registration.
        /// </para>
        /// </remarks>
        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> AsModelBinderForTypes<TLimit, TActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registration, params Type[] types)
            where TActivatorData : IConcreteActivatorData
            where TRegistrationStyle : SingleRegistrationStyle
        {
            if (registration == null)
                throw new ArgumentNullException("registration");

            if (types == null)
                throw new ArgumentNullException("types");

            var typeList = types.Where(type => type != null).ToList();
            if (typeList.Count == 0)
                throw new ArgumentException("Type list may not be empty or contain all null values.", "types");

            return registration.As<IModelBinder>().WithMetadata(AutofacModelBinderProvider.MetadataKey, typeList);
        }
    }
}
