// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// https://autofac.org
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
using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Core.Resolving.Middleware;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac
{
    /// <summary>
    /// Extension methods for registering service middleware.
    /// </summary>
    public static class ServiceMiddlewareRegistrationExtensions
    {
        private const string AnonymousDescriptor = "anonymous";

        /// <summary>
        /// Register a resolve middleware for a particular service.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="service">The service to register middleware for.</param>
        /// <param name="middleware">The middleware to register.</param>
        /// <param name="insertionMode">The insertion mode of the middleware (start or end of phase).</param>
        public static void RegisterServiceMiddleware(this ContainerBuilder builder, Service service, IResolveMiddleware middleware, MiddlewareInsertionMode insertionMode = MiddlewareInsertionMode.EndOfPhase)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (service is null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (middleware is null)
            {
                throw new ArgumentNullException(nameof(middleware));
            }

            builder.RegisterCallback(crb => crb.RegisterServiceMiddleware(service, middleware, insertionMode));
        }

        /// <summary>
        /// Register a resolve middleware for services providing a particular type.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="builder">The container builder.</param>
        /// <param name="middleware">The middleware to register.</param>
        /// <param name="insertionMode">The insertion mode of the middleware (start or end of phase).</param>
        public static void RegisterServiceMiddleware<TService>(this ContainerBuilder builder, IResolveMiddleware middleware, MiddlewareInsertionMode insertionMode = MiddlewareInsertionMode.EndOfPhase)
        {
            builder.RegisterServiceMiddleware(typeof(TService), middleware, insertionMode);
        }

        /// <summary>
        /// Register a resolve middleware for services providing a particular type.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="builder">The container builder.</param>
        /// <param name="phase">The phase of the pipeline the middleware should run at.</param>
        /// <param name="callback">
        /// A callback invoked to run your middleware.
        /// This callback takes a <see cref="ResolveRequestContext"/>, containing the context for the resolve request, plus
        /// a callback to invoke to continue the pipeline.
        /// </param>
        public static void RegisterServiceMiddleware<TService>(this ContainerBuilder builder, PipelinePhase phase, Action<ResolveRequestContext, Action<ResolveRequestContext>> callback)
        {
            builder.RegisterServiceMiddleware<TService>(AnonymousDescriptor, phase, MiddlewareInsertionMode.EndOfPhase, callback);
        }

        /// <summary>
        /// Register a resolve middleware for services providing a particular type.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="builder">The container builder.</param>
        /// <param name="descriptor">A description for the middleware; this will show up in any resolve tracing.</param>
        /// <param name="phase">The phase of the pipeline the middleware should run at.</param>
        /// <param name="callback">
        /// A callback invoked to run your middleware.
        /// This callback takes a <see cref="ResolveRequestContext"/>, containing the context for the resolve request, plus
        /// a callback to invoke to continue the pipeline.
        /// </param>
        public static void RegisterServiceMiddleware<TService>(this ContainerBuilder builder, string descriptor, PipelinePhase phase, Action<ResolveRequestContext, Action<ResolveRequestContext>> callback)
        {
            builder.RegisterServiceMiddleware<TService>(descriptor, phase, MiddlewareInsertionMode.EndOfPhase, callback);
        }

        /// <summary>
        /// Register a resolve middleware for services providing a particular type.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="builder">The container builder.</param>
        /// <param name="descriptor">A description for the middleware; this will show up in any resolve tracing.</param>
        /// <param name="phase">The phase of the pipeline the middleware should run at.</param>
        /// <param name="callback">
        /// A callback invoked to run your middleware.
        /// This callback takes a <see cref="ResolveRequestContext"/>, containing the context for the resolve request, plus
        /// a callback to invoke to continue the pipeline.
        /// </param>
        /// <param name="insertionMode">The insertion mode of the middleware (start or end of phase).</param>
        public static void RegisterServiceMiddleware<TService>(this ContainerBuilder builder, string descriptor, PipelinePhase phase, MiddlewareInsertionMode insertionMode, Action<ResolveRequestContext, Action<ResolveRequestContext>> callback)
        {
            builder.RegisterServiceMiddleware(typeof(TService), new DelegateMiddleware(descriptor, phase, callback), insertionMode);
        }

        /// <summary>
        /// Register a resolve middleware for services providing a particular type.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="serviceType">The service type.</param>
        /// <param name="middleware">The middleware to register.</param>
        /// <param name="insertionMode">The insertion mode of the middleware (start or end of phase).</param>
        public static void RegisterServiceMiddleware(this ContainerBuilder builder, Type serviceType, IResolveMiddleware middleware, MiddlewareInsertionMode insertionMode = MiddlewareInsertionMode.EndOfPhase)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (serviceType is null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (middleware is null)
            {
                throw new ArgumentNullException(nameof(middleware));
            }

            builder.RegisterServiceMiddlewareSource(new ServiceWithTypeMiddlewareSource(serviceType, middleware, insertionMode));
        }

        /// <summary>
        /// Register a source of service middleware.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="serviceMiddlewareSource">The source to add.</param>
        /// <returns>A registrar to assist with fluent addition of sources.</returns>
        public static IServiceMiddlewareSourceRegistrar RegisterServiceMiddlewareSource(this ContainerBuilder builder, IServiceMiddlewareSource serviceMiddlewareSource)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (serviceMiddlewareSource is null)
            {
                throw new ArgumentNullException(nameof(serviceMiddlewareSource));
            }

            var registrar = new ServiceMiddlewareSourceRegistrar(builder);
            return registrar.RegisterServiceMiddlewareSource(serviceMiddlewareSource);
        }
    }
}
