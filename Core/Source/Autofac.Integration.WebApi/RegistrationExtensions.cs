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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.ModelBinding;
using System.Web.Http.Validation;
using System.Web.Http.ValueProviders;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.Scanning;

namespace Autofac.Integration.WebApi
{
    /// <summary>
    /// Adds registration syntax to the <see cref="ContainerBuilder"/> type.
    /// </summary>
    public static class RegistrationExtensions
    {
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
                .Where(t => typeof(IHttpController).IsAssignableFrom(t) && t.Name.EndsWith("Controller", StringComparison.Ordinal));
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

            return registration.InstancePerMatchingLifetimeScope(AutofacWebApiDependencyResolver.ApiRequestTag);
        }

        /// <summary>
        /// Share one instance of the component within the context of a controller type.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <param name="registration">The registration to configure.</param>
        /// <param name="controllerType">The controller type.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle>
            InstancePerApiControllerType<TLimit, TActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration, Type controllerType)
        {
            if (registration == null) throw new ArgumentNullException("registration");

            return InstancePerApiControllerType(registration, controllerType, false);
        }

        /// <summary>
        /// Share one instance of the component within the context of a controller type.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <param name="registration">The registration to configure.</param>
        /// <param name="controllerType">The controller type.</param>
        /// <param name="clearExistingServices">Clear the existing list of controller level services before adding.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle>
            InstancePerApiControllerType<TLimit, TActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration, Type controllerType, bool clearExistingServices)
        {
            if (registration == null) throw new ArgumentNullException("registration");

            var services = registration.RegistrationData.Services.ToArray();
            registration.RegistrationData.ClearServices();
            var defaultService = new TypedService(typeof(TLimit));
            registration.RegistrationData.AddServices(services.Where(s => s != defaultService));

            return registration.Keyed<TLimit>(new ControllerTypeKey(controllerType))
                .WithMetadata(AutofacControllerConfigurationAttribute.ClearServiceListKey, clearExistingServices);
        }

        /// <summary>
        /// Registers the <see cref="AutofacWebApiModelBinderProvider"/>.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        public static void RegisterWebApiModelBinderProvider(this ContainerBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException("builder");

            builder.RegisterType<AutofacWebApiModelBinderProvider>()
                .As<ModelBinderProvider>()
                .SingleInstance();
        }

        /// <summary>
        /// Register types that implement <see cref="IModelBinder"/> in the provided assemblies.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="modelBinderAssemblies">Assemblies to scan for model binders.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="builder" /> or <paramref name="modelBinderAssemblies" /> is <see langword="null" />.
        /// </exception>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            RegisterWebApiModelBinders(this ContainerBuilder builder, params Assembly[] modelBinderAssemblies)
        {
            if (builder == null) throw new ArgumentNullException("builder");
            if (modelBinderAssemblies == null) throw new ArgumentNullException("modelBinderAssemblies");

            return builder.RegisterAssemblyTypes(modelBinderAssemblies)
                .Where(type => type.IsAssignableTo<IModelBinder>())
                .As<IModelBinder>()
                .SingleInstance();
        }

        /// <summary>
        /// Sets a provided registration to act as an <see cref="IModelBinder"/> for the specified list of types.
        /// </summary>
        /// <param name="registration">The registration for the type or object instance that will act as the model binder.</param>
        /// <param name="types">The list of model <see cref="Type"/> for which the <paramref name="registration" /> should be a model binder.</param>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <returns>An Autofac registration that can be modified as needed.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="registration" /> or <paramref name="types" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="types" /> is empty or contains all <see langword="null" /> values.
        /// </exception>
        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle>
            AsModelBinderForTypes<TLimit, TActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registration, params Type[] types)
            where TActivatorData : IConcreteActivatorData
            where TRegistrationStyle : SingleRegistrationStyle
        {
            if (registration == null) throw new ArgumentNullException("registration");
            if (types == null) throw new ArgumentNullException("types");

            var typeList = types.Where(type => type != null).ToList();
            if (typeList.Count == 0)
                throw new ArgumentException(RegistrationExtensionsResources.ListMustNotBeEmptyOrContainNulls, "types");

            return registration.As<IModelBinder>().WithMetadata(AutofacWebApiModelBinderProvider.MetadataKey, typeList);
        }

        /// <summary>
        /// Registers the <see cref="AutofacWebApiFilterProvider"/>.
        /// </summary>
        /// <param name="configuration">Configuration of HttpServer instances.</param>
        /// <param name="builder">The container builder.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="builder" /> or <paramref name="configuration" /> is <see langword="null" />.
        /// </exception>
        public static void RegisterWebApiFilterProvider(this ContainerBuilder builder, HttpConfiguration configuration)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            configuration.Services.RemoveAll(typeof(IFilterProvider), provider => provider is ActionDescriptorFilterProvider);

            builder.Register(c => new AutofacWebApiFilterProvider(c.Resolve<ILifetimeScope>()))
                .As<IFilterProvider>()
                .SingleInstance(); // It would be nice to scope this per request.
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAutofacActionFilter"/> for the specified controller action.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="registration">The registration.</param>
        /// <param name="actionSelector">The action selector.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<IAutofacActionFilter, IConcreteActivatorData, SingleRegistrationStyle>
            AsActionFilterFor<TController>(this IRegistrationBuilder<IAutofacActionFilter, IConcreteActivatorData, SingleRegistrationStyle> registration,
                Expression<Action<TController>> actionSelector) where TController : IHttpController
        {
            return AsFilterFor(registration, actionSelector);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAutofacActionFilter"/> for the specified controller.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="registration">The registration.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<IAutofacActionFilter, IConcreteActivatorData, SingleRegistrationStyle>
            AsActionFilterFor<TController>(this IRegistrationBuilder<IAutofacActionFilter, IConcreteActivatorData, SingleRegistrationStyle> registration)
                where TController : IHttpController
        {
            return AsFilterFor<IAutofacActionFilter, TController>(registration);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAutofacAuthorizationFilter"/> for the specified controller action.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="registration">The registration.</param>
        /// <param name="actionSelector">The action selector.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<IAutofacAuthorizationFilter, IConcreteActivatorData, SingleRegistrationStyle>
            AsAuthorizationFilterFor<TController>(this IRegistrationBuilder<IAutofacAuthorizationFilter, IConcreteActivatorData, SingleRegistrationStyle> registration,
                Expression<Action<TController>> actionSelector) where TController : IHttpController
        {
            return AsFilterFor(registration, actionSelector);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAutofacAuthorizationFilter"/> for the specified controller.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="registration">The registration.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<IAutofacAuthorizationFilter, IConcreteActivatorData, SingleRegistrationStyle>
            AsAuthorizationFilterFor<TController>(this IRegistrationBuilder<IAutofacAuthorizationFilter, IConcreteActivatorData, SingleRegistrationStyle> registration)
                where TController : IHttpController
        {
            return AsFilterFor<IAutofacAuthorizationFilter, TController>(registration);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAutofacExceptionFilter"/> for the specified controller action.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="registration">The registration.</param>
        /// <param name="actionSelector">The action selector.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<IAutofacExceptionFilter, IConcreteActivatorData, SingleRegistrationStyle>
            AsExceptionFilterFor<TController>(this IRegistrationBuilder<IAutofacExceptionFilter, IConcreteActivatorData, SingleRegistrationStyle> registration,
                Expression<Action<TController>> actionSelector) where TController : IHttpController
        {
            return AsFilterFor(registration, actionSelector);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAutofacExceptionFilter"/> for the specified controller.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="registration">The registration.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<IAutofacExceptionFilter, IConcreteActivatorData, SingleRegistrationStyle>
            AsExceptionFilterFor<TController>(this IRegistrationBuilder<IAutofacExceptionFilter, IConcreteActivatorData, SingleRegistrationStyle> registration)
                where TController : IHttpController
        {
            return AsFilterFor<IAutofacExceptionFilter, TController>(registration);
        }

        static IRegistrationBuilder<TFilter, IConcreteActivatorData, SingleRegistrationStyle>
            AsFilterFor<TFilter, TController>(IRegistrationBuilder<TFilter, IConcreteActivatorData, SingleRegistrationStyle> registration)
                where TController : IHttpController
        {
            if (registration == null) throw new ArgumentNullException("registration");

            return registration.As<TFilter>().WithMetadata(
                new FilterMetadata(typeof(TController), FilterScope.Controller, null));
        }

        static IRegistrationBuilder<TFilter, IConcreteActivatorData, SingleRegistrationStyle>
            AsFilterFor<TFilter, TController>(IRegistrationBuilder<TFilter, IConcreteActivatorData, SingleRegistrationStyle> registration, Expression<Action<TController>> actionSelector)
                where TController : IHttpController
        {
            if (registration == null) throw new ArgumentNullException("registration");
            if (actionSelector == null) throw new ArgumentNullException("actionSelector");

            return registration.As<TFilter>().WithMetadata(
                new FilterMetadata(typeof(TController), FilterScope.Action, GetMethodInfo(actionSelector)));
        }

        static MethodInfo GetMethodInfo(LambdaExpression expression)
        {
            var outermostExpression = expression.Body as MethodCallExpression;

            if (outermostExpression == null)
                throw new ArgumentException(RegistrationExtensionsResources.InvalidActionExpress);

            return outermostExpression.Method;
        }
    }
}