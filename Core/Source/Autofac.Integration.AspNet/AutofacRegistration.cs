// This software is part of the Autofac IoC container
// Copyright � 2014 Autofac Contributors
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
using System.Reflection;
using Autofac.Builder;
using Microsoft.Framework.DependencyInjection;

namespace Autofac.Integration.AspNet
{
    public static class AutofacRegistration
    {
        public static void Populate(
                this ContainerBuilder builder,
                IEnumerable<IServiceDescriptor> descriptors)
        {
            builder.Populate(descriptors, fallbackServiceProvider: null);
        }

        public static void Populate(
                this ContainerBuilder builder,
                IEnumerable<IServiceDescriptor> descriptors,
                IServiceProvider fallbackServiceProvider)
        {
            if (fallbackServiceProvider != null)
            {
                builder.RegisterSource(new ChainedRegistrationSource(fallbackServiceProvider));
            }

            builder.RegisterType<AutofacServiceProvider>().As<IServiceProvider>();
            builder.RegisterType<AutofacServiceScopeFactory>().As<IServiceScopeFactory>();

            Register(builder, descriptors);
        }

        private static void Register(
                ContainerBuilder builder,
                IEnumerable<IServiceDescriptor> descriptors)
        {
            foreach (var descriptor in descriptors)
            {
                if (descriptor.ImplementationType != null)
                {
                    // Test if the an open generic type is being registered
                    var serviceTypeInfo = descriptor.ServiceType.GetTypeInfo();
                    if (serviceTypeInfo.IsGenericTypeDefinition)
                    {
                        builder
                            .RegisterGeneric(descriptor.ImplementationType)
                            .As(descriptor.ServiceType)
                            .ConfigureLifecycle(descriptor.Lifecycle);
                    }
                    else
                    {
                        builder
                            .RegisterType(descriptor.ImplementationType)
                            .As(descriptor.ServiceType)
                            .ConfigureLifecycle(descriptor.Lifecycle);
                    }
                }
                else if (descriptor.ImplementationFactory != null)
                {
                    var registration = RegistrationBuilder.ForDelegate(descriptor.ServiceType, (context, parameters) =>
                    {
                        var serviceProvider = context.Resolve<IServiceProvider>();
                        return descriptor.ImplementationFactory(serviceProvider);
                    })
                    .ConfigureLifecycle(descriptor.Lifecycle)
                    .CreateRegistration();

                    builder.RegisterComponent(registration);
                }
                else
                {
                    builder
                        .RegisterInstance(descriptor.ImplementationInstance)
                        .As(descriptor.ServiceType)
                        .ConfigureLifecycle(descriptor.Lifecycle);
                }
            }
        }

        private static IRegistrationBuilder<object, T, U> ConfigureLifecycle<T, U>(
                this IRegistrationBuilder<object, T, U> registrationBuilder,
                LifecycleKind lifecycleKind)
        {
            switch (lifecycleKind)
            {
                case LifecycleKind.Singleton:
                    registrationBuilder.SingleInstance();
                    break;
                case LifecycleKind.Scoped:
                    registrationBuilder.InstancePerLifetimeScope();
                    break;
                case LifecycleKind.Transient:
                    registrationBuilder.InstancePerDependency();
                    break;
            }

            return registrationBuilder;
        }

        private class AutofacServiceProvider : IServiceProvider
        {
            private readonly IComponentContext _componentContext;

            public AutofacServiceProvider(IComponentContext componentContext)
            {
                _componentContext = componentContext;
            }

            public object GetService(Type serviceType)
            {
                return _componentContext.ResolveOptional(serviceType);
            }
        }

        private class AutofacServiceScopeFactory : IServiceScopeFactory
        {
            private readonly ILifetimeScope _lifetimeScope;

            public AutofacServiceScopeFactory(ILifetimeScope lifetimeScope)
            {
                _lifetimeScope = lifetimeScope;
            }

            public IServiceScope CreateScope()
            {
                return new AutofacServiceScope(_lifetimeScope.BeginLifetimeScope());
            }
        }

        private class AutofacServiceScope : IServiceScope
        {
            private readonly ILifetimeScope _lifetimeScope;
            private readonly IServiceProvider _serviceProvider;

            public AutofacServiceScope(ILifetimeScope lifetimeScope)
            {
                _lifetimeScope = lifetimeScope;
                _serviceProvider = _lifetimeScope.Resolve<IServiceProvider>();
            }

            public IServiceProvider ServiceProvider
            {
                get { return _serviceProvider; }
            }

            public void Dispose()
            {
                _lifetimeScope.Dispose();
            }
        }
    }
}
