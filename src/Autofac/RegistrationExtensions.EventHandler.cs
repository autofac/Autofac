// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Resolving.Middleware;
using Autofac.Core.Resolving.Pipeline;
using Autofac.Features.Scanning;
using Autofac.Util;

namespace Autofac
{
    /// <summary>
    /// Adds registration syntax to the <see cref="ContainerBuilder"/> type.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public static partial class RegistrationExtensions
    {
        /// <summary>
        /// Provide a handler to be called when the component is registered.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TSingleRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration add handler to.</param>
        /// <param name="handler">The handler.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle>
            OnRegistered<TLimit, TActivatorData, TSingleRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> registration,
                Action<ComponentRegisteredEventArgs> handler)
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            registration.RegistrationStyle.RegisteredHandlers.Add((s, e) => handler(e));

            return registration;
        }

        /// <summary>
        /// Provide a handler to be called when the component is registred.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration add handler to.</param>
        /// <param name="handler">The handler.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, ScanningActivatorData, TRegistrationStyle>
            OnRegistered<TLimit, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, ScanningActivatorData, TRegistrationStyle> registration,
                Action<ComponentRegisteredEventArgs> handler)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            registration.ActivatorData.ConfigurationActions.Add((t, rb) => rb.OnRegistered(handler));

            return registration;
        }

        /// <summary>
        /// Run a supplied action instead of disposing instances when they're no
        /// longer required.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set release action for.</param>
        /// <param name="releaseAction">An action to perform instead of disposing the instance.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle>
            OnRelease<TLimit, TActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registration,
                Action<TLimit> releaseAction)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            if (releaseAction == null)
            {
                throw new ArgumentNullException(nameof(releaseAction));
            }

            // Issue #677: We can't use the standard .OnActivating() handler
            // mechanism because it creates a strongly-typed "clone" of the
            // activating event args. Using a clone means a call to .ReplaceInstance()
            // on the args during activation gets lost during .OnRelease() even
            // if you keep a closure over the event args - because a later
            // .OnActivating() handler may call .ReplaceInstance() and we'll
            // have closed over the wrong thing.
            registration.ExternallyOwned();

            var middleware = new CoreEventMiddleware(ResolveEventType.OnRelease, PipelinePhase.Activation, (ctxt, next) =>
            {
                // Continue down the pipeline.
                next(ctxt);

                ctxt.ActivationScope.Disposer.AddInstanceForAsyncDisposal(new ReleaseAction<TLimit>(releaseAction, () => (TLimit)ctxt.Instance!));
            });

            registration.ResolvePipeline.Use(middleware, MiddlewareInsertionMode.StartOfPhase);

            return registration;
        }

        /// <summary>
        /// Run a supplied async action instead of disposing instances when they're no
        /// longer required.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set release action for.</param>
        /// <param name="releaseAction">
        /// An action to perform instead of disposing the instance.
        /// The release/disposal process will not continue until the returned task completes.
        /// </param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle>
            OnRelease<TLimit, TActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registration,
                Func<TLimit, ValueTask> releaseAction)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            if (releaseAction == null)
            {
                throw new ArgumentNullException(nameof(releaseAction));
            }

            registration.ExternallyOwned();

            var middleware = new CoreEventMiddleware(ResolveEventType.OnRelease, PipelinePhase.Activation, (ctxt, next) =>
            {
                // Continue down the pipeline.
                next(ctxt);

                // Use an async release action that invokes the release callback in a proper async/await flow if someone
                // is using actual async disposal.
                ctxt.ActivationScope.Disposer.AddInstanceForAsyncDisposal(new AsyncReleaseAction<TLimit>(releaseAction, () => (TLimit)ctxt.Instance!));
            });

            registration.ResolvePipeline.Use(middleware, MiddlewareInsertionMode.StartOfPhase);

            return registration;
        }
    }
}
