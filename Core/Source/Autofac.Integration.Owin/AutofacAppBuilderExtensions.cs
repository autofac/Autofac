<<<<<<< HEAD:Core/Source/Autofac.Integration.Owin/OwinExtensions.cs
﻿// This software is part of the Autofac IoC container
// Copyright © 2014 Autofac Contributors
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
using System.ComponentModel;
using System.Linq;
using System.Security;
using Autofac.Core;
using Microsoft.Owin;
using Owin;

namespace Autofac.Integration.Owin
{
    /// <summary>
    /// Extension methods for configuring the OWIN pipeline.
    /// </summary>
    [SecuritySafeCritical]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class OwinExtensions
    {
        const string MiddlewareRegisteredKey = "AutofacMiddelwareRegistered";

        /// <summary>
        /// Adds a component to the OWIN pipeline for using Autofac dependency injection with middleware.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="container">The Autofac container.</param>
        /// <returns>The application builder.</returns>
        [SecuritySafeCritical]
        public static IAppBuilder UseAutofacMiddleware(this IAppBuilder app, IContainer container)
        {
            if (app.Properties.ContainsKey(MiddlewareRegisteredKey)) return app;

            app.Use(async (context, next) =>
            {
                using (var lifetimeScope = container.BeginLifetimeScope(Constants.LifetimeScopeTag,
                    b => b.RegisterInstance(context).As<IOwinContext>()))
                {
                    context.Set(Constants.OwinLifetimeScopeKey, lifetimeScope);
                    await next();
                }
            });

            UseMiddlewareFromContainer(app, container);

            app.Properties.Add(MiddlewareRegisteredKey, true);

            return app;
        }

        /// <summary>
        /// Gets the current Autofac lifetime scope from the OWIN context.
        /// </summary>
        /// <param name="context">The OWIN context.</param>
        /// <returns>The current lifetime scope.</returns>
        [SecuritySafeCritical]
        public static ILifetimeScope GetAutofacLifetimeScope(this IOwinContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            return context.Get<ILifetimeScope>(Constants.OwinLifetimeScopeKey);
        }

        [SecuritySafeCritical]
        static void UseMiddlewareFromContainer(this IAppBuilder app, IComponentContext container)
        {
            var services = container.ComponentRegistry.Registrations.SelectMany(r => r.Services)
                .OfType<TypedService>()
                .Where(s => s.ServiceType.IsAssignableTo<OwinMiddleware>() && !s.ServiceType.IsAbstract)
                .Select(service => typeof(AutofacMiddleware<>).MakeGenericType(service.ServiceType))
                .Where(serviceType => !container.IsRegistered(serviceType));

            var typedServices = services.ToArray();
            if (!typedServices.Any()) return;

            foreach (var typedService in typedServices)
                app.Use(typedService);
        }
    }
}
=======
﻿// This software is part of the Autofac IoC container
// Copyright © 2014 Autofac Contributors
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

using System.ComponentModel;
using System.Linq;
using System.Security;
using Autofac;
using Autofac.Core;
using Autofac.Integration.Owin;
using Microsoft.Owin;

namespace Owin
{
    /// <summary>
    /// Extension methods for configuring Autofac within the OWIN pipeline.
    /// </summary>
    [SecuritySafeCritical]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class OwinExtensions
    {
        /// <summary>
        /// Adds a component to the OWIN pipeline for using Autofac dependency injection with middleware.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="container">The Autofac application lifetime scope/container.</param>
        /// <returns>The application builder.</returns>
        [SecuritySafeCritical]
        public static IAppBuilder UseAutofacMiddleware(this IAppBuilder app, ILifetimeScope container)
        {
            app.Use(async (context, next) =>
            {
                using (var lifetimeScope = container.BeginLifetimeScope(Constants.LifetimeScopeTag,
                    b => b.RegisterInstance(context).As<IOwinContext>()))
                {
                    context.Set(Constants.OwinLifetimeScopeKey, lifetimeScope);
                    await next();
                }
            });

            return UseMiddlewareFromContainer(app, container);
        }

        [SecuritySafeCritical]
        static IAppBuilder UseMiddlewareFromContainer(this IAppBuilder app, IComponentContext container)
        {
            var services = container.ComponentRegistry.Registrations.SelectMany(r => r.Services)
                .OfType<TypedService>()
                .Where(s => s.ServiceType.IsAssignableTo<OwinMiddleware>() && !s.ServiceType.IsAbstract)
                .Select(service => typeof(AutofacMiddleware<>).MakeGenericType(service.ServiceType))
                .Where(serviceType => !container.IsRegistered(serviceType));

            var typedServices = services.ToArray();
            if (!typedServices.Any()) return app;

            foreach (var typedService in typedServices)
                app.Use(typedService);

            return app;
        }
    }
}
>>>>>>> e3e9244422579fd2ba98755f752cd74beff01c3c:Core/Source/Autofac.Integration.Owin/AutofacAppBuilderExtensions.cs
