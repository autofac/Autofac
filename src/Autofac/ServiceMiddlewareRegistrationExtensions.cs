﻿// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Core.Resolving.Middleware;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac;

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
    /// <param name="phase">The phase of the pipeline the middleware should run at.</param>
    /// <param name="callback">
    /// A callback invoked to run your middleware.
    /// This callback takes a <see cref="ResolveRequestContext"/>, containing the context for the resolve request, plus
    /// a callback to invoke to continue the pipeline.
    /// </param>
    public static void RegisterServiceMiddleware(this ContainerBuilder builder, Type serviceType, PipelinePhase phase, Action<ResolveRequestContext, Action<ResolveRequestContext>> callback)
    {
        builder.RegisterServiceMiddleware(serviceType, AnonymousDescriptor, phase, MiddlewareInsertionMode.EndOfPhase, callback);
    }

    /// <summary>
    /// Register a resolve middleware for services providing a particular type.
    /// </summary>
    /// <param name="builder">The container builder.</param>
    /// <param name="serviceType">The service type.</param>
    /// <param name="descriptor">A description for the middleware; this will show up in any resolve tracing.</param>
    /// <param name="phase">The phase of the pipeline the middleware should run at.</param>
    /// <param name="callback">
    /// A callback invoked to run your middleware.
    /// This callback takes a <see cref="ResolveRequestContext"/>, containing the context for the resolve request, plus
    /// a callback to invoke to continue the pipeline.
    /// </param>
    public static void RegisterServiceMiddleware(this ContainerBuilder builder, Type serviceType, string descriptor, PipelinePhase phase, Action<ResolveRequestContext, Action<ResolveRequestContext>> callback)
    {
        builder.RegisterServiceMiddleware(serviceType, descriptor, phase, MiddlewareInsertionMode.EndOfPhase, callback);
    }

    /// <summary>
    /// Register a resolve middleware for services providing a particular type.
    /// </summary>
    /// <param name="builder">The container builder.</param>
    /// <param name="serviceType">The service type.</param>
    /// <param name="descriptor">A description for the middleware; this will show up in any resolve tracing.</param>
    /// <param name="phase">The phase of the pipeline the middleware should run at.</param>
    /// <param name="callback">
    /// A callback invoked to run your middleware.
    /// This callback takes a <see cref="ResolveRequestContext"/>, containing the context for the resolve request, plus
    /// a callback to invoke to continue the pipeline.
    /// </param>
    /// <param name="insertionMode">The insertion mode of the middleware (start or end of phase).</param>
    public static void RegisterServiceMiddleware(this ContainerBuilder builder, Type serviceType, string descriptor, PipelinePhase phase, MiddlewareInsertionMode insertionMode, Action<ResolveRequestContext, Action<ResolveRequestContext>> callback)
    {
        builder.RegisterServiceMiddleware(serviceType, new DelegateMiddleware(descriptor, phase, callback), insertionMode);
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
