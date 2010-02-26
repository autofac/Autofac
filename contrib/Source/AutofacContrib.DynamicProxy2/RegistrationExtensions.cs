using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Castle.Core.Interceptor;
using Castle.DynamicProxy;

namespace AutofacContrib.DynamicProxy2
{
    public static class RegistrationExtensions
    {
        static readonly ProxyGenerator ProxyGenerator = new ProxyGenerator();
        const string InterceptorsPropertyName = "AutofacContrib.DynamicProxy2.RegistrationExtensions.InterceptorsPropertyName";
        static readonly IEnumerable<Service> EmptyServices = new Service[0];

        public static RegistrationBuilder<TLimit, ConcreteReflectionActivatorData, TRegistrationStyle>
            EnableInterceptors<TLimit, TRegistrationStyle>(
                this RegistrationBuilder<TLimit, ConcreteReflectionActivatorData, TRegistrationStyle> registration)
        {
            registration.ActivatorData.ImplementationType =
                ProxyGenerator.ProxyBuilder.CreateClassProxyType(
                    registration.ActivatorData.ImplementationType, new Type[0], ProxyGenerationOptions.Default);

            registration.OnPreparing(e =>
            {
                e.Parameters = new Parameter[] {
                    new PositionalParameter(0,
                        GetInterceptorServices(e.Component).Select(s => e.Context.Resolve(s)).Cast<IInterceptor>().ToArray())
                }.Concat(e.Parameters).ToArray();
            });

            return registration;
        }

        public static RegistrationBuilder<TLimit, SimpleActivatorData, TSingleRegistrationStyle>
            EnableInterceptors<TLimit, TSingleRegistrationStyle>(
                this RegistrationBuilder<TLimit, SimpleActivatorData, TSingleRegistrationStyle> registration)
        {
            registration.RegistrationData.ActivatingHandlers.Add((sender, e) =>
            {
                var proxiedInterfaces = e.Instance.GetType().GetInterfaces().Where(i => i.IsPublic);

                if (!proxiedInterfaces.Any())
                    throw new ArgumentException(
                        string.Format(CultureInfo.CurrentCulture, RegistrationExtensionsResources.ComponentDoesNotProvideInterceptibleServices, registration));

                var theInterface = proxiedInterfaces.First();
                var interfaces = proxiedInterfaces.Skip(1).ToArray();

                var interceptors = GetInterceptorServices(e.Component).Select(s => e.Context.Resolve(s)).Cast<IInterceptor>().ToArray();

                e.Instance = ProxyGenerator.CreateInterfaceProxyWithTarget(theInterface, interfaces, e.Instance, interceptors);
            });

            return registration;
        }

        static IEnumerable<Service> GetInterceptorServices(IComponentRegistration registration)
        {
            if (registration == null)
                throw new ArgumentNullException("registration");

            var result = EmptyServices;

            object services;
            if (registration.Metadata.TryGetValue(InterceptorsPropertyName, out services))
                result = result.Concat((IEnumerable<Service>)services);

            var implType = registration.Activator.LimitType;
            if (implType.IsClass)
            {
                result = result.Concat(implType
                    .GetCustomAttributes(typeof(InterceptAttribute), true)
                    .Cast<InterceptAttribute>()
                    .Select(att => att.InterceptorService)
                    .ToList());
            }

            return result;
        }

        public static RegistrationBuilder<TLimit, TActivatorData, TStyle>
            InterceptedBy<TLimit, TActivatorData, TStyle>(
                this RegistrationBuilder<TLimit, TActivatorData, TStyle> builder,
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

        public static RegistrationBuilder<TLimit, TActivatorData, TStyle>
            InterceptedBy<TLimit, TActivatorData, TStyle>(
                this RegistrationBuilder<TLimit, TActivatorData, TStyle> builder,
                params string[] interceptorServiceNames)
        {
            if (interceptorServiceNames == null || interceptorServiceNames.Any(n => n == null))
                throw new ArgumentNullException("interceptorServiceNames");

            return InterceptedBy(builder, interceptorServiceNames.Select(n => new NamedService(n, typeof(IInterceptor))).ToArray());
        }

        public static RegistrationBuilder<TLimit, TActivatorData, TStyle>
            InterceptedBy<TLimit, TActivatorData, TStyle>(
                this RegistrationBuilder<TLimit, TActivatorData, TStyle> builder,
                params Type[] interceptorServiceTypes)
        {
            if (interceptorServiceTypes == null || interceptorServiceTypes.Any(t => t == null))
                throw new ArgumentNullException("interceptorServiceTypes");

            return InterceptedBy(builder, interceptorServiceTypes.Select(t => new TypedService(t)).ToArray());
        }
    }
}
