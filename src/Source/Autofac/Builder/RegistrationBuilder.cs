// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2008 Autofac Contributors
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
using Autofac.Core.Lifetime;
using Autofac.Util;
using Autofac.Core.Activators.Reflection;
using Autofac.Core;

namespace Autofac.Builder
{
    /// <summary>
    /// Data structure used to construct registrations.
    /// </summary>
    /// <typeparam name="TLimit">The most specific type to which instances of the registration
    /// can be cast.</typeparam>
    /// <typeparam name="TActivatorData">Activator builder type.</typeparam>
    /// <typeparam name="TRegistrationStyle">Registration style type.</typeparam>
    public class RegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle>
    {
        readonly TActivatorData _activatorData;
        readonly TRegistrationStyle _registrationStyle;
        readonly RegistrationData _registrationData = new RegistrationData();

        /// <summary>
        /// Create a RegistrationBuilder.
        /// </summary>
        /// <param name="activatorData">Activator builder.</param>
        /// <param name="style">Registration style.</param>
        public RegistrationBuilder(TActivatorData activatorData, TRegistrationStyle style)
        {
            Enforce.ArgumentNotNull((object)activatorData, "activatorData");
            Enforce.ArgumentNotNull((object)style, "style");
            _activatorData = activatorData;
            _registrationStyle = style;
        }

        /// <summary>
        /// Gets the activator data.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public TActivatorData ActivatorData { get { return _activatorData; } }

        /// <summary>
        /// Gets the registration style.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public TRegistrationStyle RegistrationStyle { get { return _registrationStyle; } }

        /// <summary>
        /// Gets the registration data.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public RegistrationData RegistrationData { get { return _registrationData; } }

        /// <summary>
        /// Configure the component so that instances are never disposed by the container.
        /// </summary>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public RegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> ExternallyOwned()
        {
            RegistrationData.Ownership = InstanceOwnership.ExternallyOwned;
            return this;
        }

        /// <summary>
        /// Configure the component so that instances that support IDisposable are
        /// disposed by the container (default.)
        /// </summary>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public RegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> OwnedByLifetimeScope()
        {
            RegistrationData.Ownership = InstanceOwnership.OwnedByLifetimeScope;
            return this;
        }

        /// <summary>
        /// Configure the component so that every dependent component or call to Resolve()
        /// gets a new, unique instance (default.)
        /// </summary>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public RegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> UniqueInstances()
        {
            RegistrationData.Sharing = InstanceSharing.None;
            RegistrationData.Lifetime = new CurrentScopeLifetime();
            return this;
        }

        /// <summary>
        /// Configure the component so that every dependent component or call to Resolve()
        /// gets the same, shared instance.
        /// </summary>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public RegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> SingleInstance()
        {
            RegistrationData.Sharing = InstanceSharing.Shared;
            RegistrationData.Lifetime = new RootScopeLifetime();
            return this;
        }

        /// <summary>
        /// Configure the component so that every dependent component or call to Resolve()
        /// within a single ILifetimeScope gets the same, shared instance. Dependent components in
        /// different lifetime scopes will get different instances.
        /// </summary>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public RegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerLifetimeScope()
        {
            RegistrationData.Sharing = InstanceSharing.Shared;
            RegistrationData.Lifetime = new CurrentScopeLifetime();
            return this;
        }

        /// <summary>
        /// Configure the component so that every dependent component or call to Resolve()
        /// within a ILifetimeScope tagged with the provided tag value gets the same, shared instance.
        /// Dependent components in lifetime scopes that are children of the tagged scope will
        /// share the parent's instance. If no appropriately tagged scope can be found in the
        /// hierarchy an <see cref="DependencyResolutionException"/> is thrown.
        /// </summary>
        /// <param name="lifetimeScopeTag">Tag applied to matching lifetime scopes.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public RegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerMatchingLifetimeScope(object lifetimeScopeTag)
        {
            Enforce.ArgumentNotNull(lifetimeScopeTag, "lifetimeScopeTag");
            RegistrationData.Sharing = InstanceSharing.Shared;
            RegistrationData.Lifetime = new MatchingScopeLifetime(scope => lifetimeScopeTag.Equals(scope.Tag));
            return this;
        }

        /// <summary>
        /// Configure the services that the component will provide. The generic parameter(s) to As()
        /// will be exposed as TypedService instances.
        /// </summary>
        /// <typeparam name="TService">Service type.</typeparam>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public RegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> As<TService>()
        {
            return As(new TypedService(typeof(TService)));
        }

        /// <summary>
        /// Configure the services that the component will provide. The generic parameter(s) to As()
        /// will be exposed as TypedService instances.
        /// </summary>
        /// <typeparam name="TService1">Service type.</typeparam>
        /// <typeparam name="TService2">Service type.</typeparam>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public RegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> As<TService1, TService2>()
        {
            return As(new TypedService(typeof(TService1)), new TypedService(typeof(TService2)));
        }

        /// <summary>
        /// Configure the services that the component will provide. The generic parameter(s) to As()
        /// will be exposed as TypedService instances.
        /// </summary>
        /// <typeparam name="TService1">Service type.</typeparam>
        /// <typeparam name="TService2">Service type.</typeparam>
        /// <typeparam name="TService3">Service type.</typeparam>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public RegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> As<TService1, TService2, TService3>()
        {
            return As(new TypedService(typeof(TService1)), new TypedService(typeof(TService2)), new TypedService(typeof(TService3)));
        }

        /// <summary>
        /// Configure the services that the component will provide.
        /// </summary>
        /// <param name="services">Service types to expose.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public RegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> As(params Type[] services)
        {
            return As(services.Select(t => new TypedService(t)).Cast<Service>().ToArray());
        }

        /// <summary>
        /// Configure the services that the component will provide.
        /// </summary>
        /// <param name="services">Services to expose.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public RegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> As(params Service[] services)
        {
            Enforce.ArgumentNotNull(services, "services");

            foreach (var service in services)
                RegistrationData.Services.Add(service);

            return this;
        }

        /// <summary>
        /// Configure the services that the component will provide.
        /// </summary>
        /// <param name="serviceName">Named service to associate with the component.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public RegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> Named(string serviceName)
        {
            Enforce.ArgumentNotNull(serviceName, "serviceName");

            return As(new NamedService(serviceName));
        }

        /// <summary>
        /// Add a handler for the Preparing event. This event allows manipulating of the parameters
        /// that will be provided to the component.
        /// </summary>
        /// <param name="handler">The event handler.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public RegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> OnPreparing(Action<PreparingEventArgs<TLimit>> handler)
        {
            Enforce.ArgumentNotNull(handler, "handler");
            RegistrationData.PreparingHandlers.Add((s, e) =>
            {
                handler(new PreparingEventArgs<TLimit>(e.Context, e.Component, e.Parameters));
            });
            return this;
        }

        /// <summary>
        /// Add a handler for the Activating event.
        /// </summary>
        /// <param name="handler">The event handler.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public RegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> OnActivating(Action<ActivatingEventArgs<TLimit>> handler)
        {
            Enforce.ArgumentNotNull(handler, "handler");
            RegistrationData.ActivatingHandlers.Add((s, e) =>
            {
                handler(new ActivatingEventArgs<TLimit>(e.Context, e.Component, (TLimit)e.Instance));
            });
            return this;
        }

        /// <summary>
        /// Add a handler for the Activated event.
        /// </summary>
        /// <param name="handler">The event handler.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public RegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> OnActivated(Action<ActivatedEventArgs<TLimit>> handler)
        {
            Enforce.ArgumentNotNull(handler, "handler");
            RegistrationData.ActivatedHandlers.Add((s, e) =>
            {
                handler(new ActivatedEventArgs<TLimit>(e.Context, e.Component, (TLimit)e.Instance));
            });
            return this;
        }

        /// <summary>
        /// Configure the component so that any properties whose types are registered in the
        /// container will be wired to instances of the appropriate service.
        /// </summary>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public RegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> PropertiesAutowired()
        {
            var injector = new AutowiringPropertyInjector();
            RegistrationData.ActivatingHandlers.Add((s, e) =>
            {
                injector.InjectProperties(e.Context, e.Instance, true);
            });
            return this;
        }

        /// <summary>
        /// Configure the component so that any properties whose types are registered in the
        /// container will be wired to instances of the appropriate service.
        /// </summary>
        /// <param name="allowCircularDependencies">If set to true, the properties won't be wired until
        /// after the component has been activated. This allows property-property and constructor-property
        /// circularities in the dependency graph.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public RegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> PropertiesAutowired(bool allowCircularDependencies)
        {
            if (allowCircularDependencies)
            {
                var injector = new AutowiringPropertyInjector();
                RegistrationData.ActivatedHandlers.Add((s, e) =>
                {
                    injector.InjectProperties(e.Context, e.Instance, true);
                });
                return this;
            }
            else
            {
                return PropertiesAutowired();
            }
        }

        /// <summary>
        /// Associates data with the component.
        /// </summary>
        /// <param name="key">Key by which the data can be located.</param>
        /// <param name="value">The data value.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public RegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> WithExtendedProperty(string key, object value)
        {
            Enforce.ArgumentNotNull(key, "key");

            RegistrationData.ExtendedProperties.Add(key, value);

            return this;
        }
    }
}
