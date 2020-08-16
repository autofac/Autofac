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
