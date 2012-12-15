// This software is part of the Autofac IoC container
// Copyright © 2012 Autofac Contributors
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
using System.Runtime.Remoting;
using System.Security;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.Scanning;
using Castle.DynamicProxy;

namespace Autofac.Extras.DynamicProxy2
{
    [SecuritySafeCritical]
    public static class RegistrationExtensions
    {
        static readonly ProxyGenerator ProxyGenerator = new ProxyGenerator();
        const string InterceptorsPropertyName = "Autofac.Extras.DynamicProxy2.RegistrationExtensions.InterceptorsPropertyName";
        static readonly IEnumerable<Service> EmptyServices = new Service[0];

        /// <summary>
        /// Enable class interception on the target type. Interceptors will be determined
        /// via Intercept attributes on the class or added with InterceptedBy().
        /// Only virtual methods can be intercepted this way.
        /// </summary>
        /// <typeparam name="TLimit"></typeparam>
        /// <typeparam name="TRegistrationStyle"></typeparam>
        /// <param name="registration"></param>
        /// <returns></returns>
        public static IRegistrationBuilder<TLimit, ScanningActivatorData, TRegistrationStyle>
            EnableClassInterceptors<TLimit, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, ScanningActivatorData, TRegistrationStyle> registration)
        {
            if (registration == null) throw new ArgumentNullException("registration");
            registration.ActivatorData.ConfigurationActions.Add(
                (t, rb) => rb.EnableClassInterceptors());
            return registration;
        }

        /// <summary>
        /// Enable class interception on the target type. Interceptors will be determined
        /// via Intercept attributes on the class or added with InterceptedBy().
        /// Only virtual methods can be intercepted this way.
        /// </summary>
        /// <typeparam name="TLimit"></typeparam>
        /// <typeparam name="TRegistrationStyle"></typeparam>
        /// <typeparam name="TConcreteReflectionActivatorData"></typeparam>
        /// <param name="registration"></param>
        /// <returns></returns>
        public static IRegistrationBuilder<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle>
            EnableClassInterceptors<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle> registration)
            where TConcreteReflectionActivatorData : ConcreteReflectionActivatorData
        {
            if (registration == null)
            {
                throw new ArgumentNullException("registration");
            }
            registration.ActivatorData.ImplementationType =
                ProxyGenerator.ProxyBuilder.CreateClassProxyType(
                    registration.ActivatorData.ImplementationType, new Type[0], ProxyGenerationOptions.Default);

            registration.OnPreparing(e =>
            {
                e.Parameters = new Parameter[] {
                    new PositionalParameter(0, GetInterceptorServices(e.Component, registration.ActivatorData.ImplementationType)
                        .Select(s => e.Context.ResolveService(s))
                        .Cast<IInterceptor>()
                        .ToArray())
                }.Concat(e.Parameters).ToArray();
            });

            return registration;
        }

        /// <summary>
        /// Enable interface interception on the target type. Interceptors will be determined
        /// via Intercept attributes on the class or interface, or added with InterceptedBy() calls.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TSingleRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to apply interception to.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle>
            EnableInterfaceInterceptors<TLimit, TActivatorData, TSingleRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> registration)
        {
            if (registration == null)
            {
                throw new ArgumentNullException("registration");
            }
            registration.RegistrationData.ActivatingHandlers.Add((sender, e) =>
            {
                EnsureInterfaceInterceptionApplies(e.Component);

                var proxiedInterfaces = e.Instance.GetType().GetInterfaces().Where(i => i.IsVisible).ToArray();

                if (!proxiedInterfaces.Any())
                    return;

                var theInterface = proxiedInterfaces.First();
                var interfaces = proxiedInterfaces.Skip(1).ToArray();

                var interceptors = GetInterceptorServices(e.Component, e.Instance.GetType())
                    .Select(s => e.Context.ResolveService(s))
                    .Cast<IInterceptor>()
                    .ToArray();

                e.Instance = ProxyGenerator.CreateInterfaceProxyWithTarget(theInterface, interfaces, e.Instance, interceptors);
            });

            return registration;
        }

        /// <summary>
        /// Intercepts the interface of a transparent proxy (such as WCF channel factory based clients).
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TSingleRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to apply interception to.</param>
        /// <param name="additionalInterfacesToProxy">Additional interface types. Calls to their members will be proxied as well.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle>
            InterceptTransparentProxy<TLimit, TActivatorData, TSingleRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> registration, params Type[] additionalInterfacesToProxy)
        {
            if (registration == null)
            {
                throw new ArgumentNullException("registration");
            }
            registration.RegistrationData.ActivatingHandlers.Add((sender, e) =>
            {
                EnsureInterfaceInterceptionApplies(e.Component);

                if (!RemotingServices.IsTransparentProxy(e.Instance))
                {
                    throw new DependencyResolutionException(string.Format(
                        CultureInfo.CurrentCulture, RegistrationExtensionsResources.TypeIsNotTransparentProxy, e.Instance.GetType().FullName));
                }

                if (!e.Instance.GetType().IsInterface)
                {
                    throw new DependencyResolutionException(string.Format(
                        CultureInfo.CurrentCulture, RegistrationExtensionsResources.TransparentProxyIsNotInterface, e.Instance.GetType().FullName));
                }

                if (additionalInterfacesToProxy.Any())
                {
                    var remotingTypeInfo = (IRemotingTypeInfo)RemotingServices.GetRealProxy(e.Instance);

                    var invalidInterfaces = additionalInterfacesToProxy
                        .Where(i => !remotingTypeInfo.CanCastTo(i, e.Instance))
                        .ToArray();

                    if (invalidInterfaces.Any())
                    {
                        var message = string.Format(CultureInfo.CurrentCulture, RegistrationExtensionsResources.InterfaceNotSupportedByTransparentProxy, 
                            string.Join(", ", invalidInterfaces.Select(i => i.FullName)));
                        throw new DependencyResolutionException(message);
                    }
                }

                var interceptors = GetInterceptorServices(e.Component, e.Instance.GetType())
                    .Select(s => e.Context.ResolveService(s))
                    .Cast<IInterceptor>()
                    .ToArray();

                e.Instance = ProxyGenerator.CreateInterfaceProxyWithTargetInterface(
                    e.Instance.GetType(), additionalInterfacesToProxy, e.Instance, interceptors);
            });

            return registration;
        }

        static void EnsureInterfaceInterceptionApplies(IComponentRegistration componentRegistration)
        {
            if (componentRegistration.Services
                .OfType<IServiceWithType>()
                .Any(swt => !swt.ServiceType.IsInterface || !swt.ServiceType.IsVisible))
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
                    RegistrationExtensionsResources.InterfaceProxyingOnlySupportsInterfaceServices,
                    componentRegistration));
        }

        static IEnumerable<Service> GetInterceptorServices(IComponentRegistration registration, Type implType)
        {
            if (registration == null) throw new ArgumentNullException("registration");
            if (implType == null) throw new ArgumentNullException("implType");

            var result = EmptyServices;

            object services;
            if (registration.Metadata.TryGetValue(InterceptorsPropertyName, out services))
                result = result.Concat((IEnumerable<Service>)services);

            if (implType.IsClass)
            {
                result = result.Concat(implType
                    .GetCustomAttributes(typeof(InterceptAttribute), true)
                    .Cast<InterceptAttribute>()
                    .Select(att => att.InterceptorService));

                result = result.Concat(implType.GetInterfaces()
                    .SelectMany(i => i.GetCustomAttributes(typeof(InterceptAttribute), true))
                    .Cast<InterceptAttribute>()
                    .Select(att => att.InterceptorService));
            }

            return result.ToArray();
        }

        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle>
            InterceptedBy<TLimit, TActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TStyle> builder,
                params Service[] interceptorServices)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");

            if (interceptorServices == null || interceptorServices.Any(s => s == null))
                throw new ArgumentNullException("interceptorServices");

            object existing;
            if (builder.RegistrationData.Metadata.TryGetValue(InterceptorsPropertyName, out existing))
                builder.RegistrationData.Metadata[InterceptorsPropertyName] =
                    ((IEnumerable<Service>)existing).Concat(interceptorServices).Distinct();
            else
                builder.RegistrationData.Metadata.Add(InterceptorsPropertyName, interceptorServices);

            return builder;
        }

        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle>
            InterceptedBy<TLimit, TActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TStyle> builder,
                params string[] interceptorServiceNames)
        {
            if (interceptorServiceNames == null || interceptorServiceNames.Any(n => n == null))
                throw new ArgumentNullException("interceptorServiceNames");

            return InterceptedBy(builder, interceptorServiceNames.Select(n => new KeyedService(n, typeof(IInterceptor))).ToArray());
        }

        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle>
            InterceptedBy<TLimit, TActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TStyle> builder,
                params Type[] interceptorServiceTypes)
        {
            if (interceptorServiceTypes == null || interceptorServiceTypes.Any(t => t == null))
                throw new ArgumentNullException("interceptorServiceTypes");

            return InterceptedBy(builder, interceptorServiceTypes.Select(t => new TypedService(t)).ToArray());
        }
    }
}
