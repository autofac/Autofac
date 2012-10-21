// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
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
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Autofac.Core.Lifetime;
using Autofac.Features.Metadata;

namespace Autofac.Builder
{
    class RegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> : IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle>, IHideObjectMembers
    {
        readonly TActivatorData _activatorData;
        readonly TRegistrationStyle _registrationStyle;
        readonly RegistrationData _registrationData;

        public RegistrationBuilder(Service defaultService, TActivatorData activatorData, TRegistrationStyle style)
        {
            if (defaultService == null) throw new ArgumentNullException("defaultService");
            if ((object)activatorData == null) throw new ArgumentNullException("activatorData");
            if ((object)style == null) throw new ArgumentNullException("style");

            _activatorData = activatorData;
            _registrationStyle = style;
            _registrationData = new RegistrationData(defaultService);
        }

        /// <summary>
        /// The activator data.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public TActivatorData ActivatorData { get { return _activatorData; } }

        /// <summary>
        /// The registration style.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public TRegistrationStyle RegistrationStyle { get { return _registrationStyle; } }

        /// <summary>
        /// The registration data.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public RegistrationData RegistrationData { get { return _registrationData; } }

        /// <summary>
        /// Configure the component so that instances are never disposed by the container.
        /// </summary>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> ExternallyOwned()
        {
            RegistrationData.Ownership = InstanceOwnership.ExternallyOwned;
            return this;
        }

        /// <summary>
        /// Configure the component so that instances that support IDisposable are
        /// disposed by the container (default.)
        /// </summary>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> OwnedByLifetimeScope()
        {
            RegistrationData.Ownership = InstanceOwnership.OwnedByLifetimeScope;
            return this;
        }

        /// <summary>
        /// Configure the component so that every dependent component or call to Resolve()
        /// gets a new, unique instance (default.)
        /// </summary>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerDependency()
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
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> SingleInstance()
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
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerLifetimeScope()
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
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerMatchingLifetimeScope(object lifetimeScopeTag)
        {
            if (lifetimeScopeTag == null) throw new ArgumentNullException("lifetimeScopeTag");
            RegistrationData.Sharing = InstanceSharing.Shared;
            RegistrationData.Lifetime = new MatchingScopeLifetime(lifetimeScopeTag);
            return this;
        }

        /// <summary>
        /// Configure the component so that every dependent component or call to Resolve()
        /// within a ILifetimeScope with a tag matching the provided expression gets the same, shared instance.
        /// Dependent components in lifetime scopes that are children of the tagged scope will
        /// share the parent's instance. If no appropriately tagged scope can be found in the
        /// hierarchy an <see cref="DependencyResolutionException"/> is thrown.
        /// </summary>
        /// <param name="matchExpression">Expression describing scopes that will match.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerMatchingLifetimeScope(Expression<Func<object, bool>> matchExpression)
        {
            if (matchExpression == null) throw new ArgumentNullException("matchExpression");
            RegistrationData.Sharing = InstanceSharing.Shared;
            RegistrationData.Lifetime = new MatchingScopeLifetime(matchExpression);
            return this;
        }

        /// <summary>
        /// Configure the component so that every dependent component or call to Resolve()
        /// within a ILifetimeScope created by an owned instance gets the same, shared instance.
        /// Dependent components in lifetime scopes that are children of the owned instance scope will
        /// share the parent's instance. If no appropriate owned instance scope can be found in the
        /// hierarchy an <see cref="DependencyResolutionException"/> is thrown.
        /// </summary>
        /// <typeparam name="TService">The service type provided by the component.</typeparam>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerOwned<TService>()
        {
            return InstancePerOwned(typeof(TService));
        }

        /// <summary>
        /// Configure the component so that every dependent component or call to Resolve()
        /// within a ILifetimeScope created by an owned instance gets the same, shared instance.
        /// Dependent components in lifetime scopes that are children of the owned instance scope will
        /// share the parent's instance. If no appropriate owned instance scope can be found in the
        /// hierarchy an <see cref="DependencyResolutionException"/> is thrown.
        /// </summary>
        /// <param name="serviceType">The service type provided by the component.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerOwned(Type serviceType)
        {
            return InstancePerMatchingLifetimeScope(new TypedService(serviceType));
        }

        /// <summary>
        /// Configure the component so that every dependent component or call to Resolve()
        /// within a ILifetimeScope created by an owned instance gets the same, shared instance.
        /// Dependent components in lifetime scopes that are children of the owned instance scope will
        /// share the parent's instance. If no appropriate owned instance scope can be found in the
        /// hierarchy an <see cref="DependencyResolutionException"/> is thrown.
        /// </summary>
        /// <typeparam name="TService">The service type provided by the component.</typeparam>
        /// <param name="serviceKey">Key to associate with the component.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerOwned<TService>(object serviceKey)
        {
            return InstancePerOwned(serviceKey, typeof(TService));
        }

        /// <summary>
        /// Configure the component so that every dependent component or call to Resolve()
        /// within a ILifetimeScope created by an owned instance gets the same, shared instance.
        /// Dependent components in lifetime scopes that are children of the owned instance scope will
        /// share the parent's instance. If no appropriate owned instance scope can be found in the
        /// hierarchy an <see cref="DependencyResolutionException"/> is thrown.
        /// </summary>
        /// <param name="serviceKey">Key to associate with the component.</param>
        /// <param name="serviceType">The service type provided by the component.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerOwned(object serviceKey, Type serviceType)
        {
            return InstancePerMatchingLifetimeScope(new KeyedService(serviceKey, serviceType));
        }

        /// <summary>
        /// Configure the services that the component will provide. The generic parameter(s) to As()
        /// will be exposed as TypedService instances.
        /// </summary>
        /// <typeparam name="TService">Service type.</typeparam>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> As<TService>()
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
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> As<TService1, TService2>()
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
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> As<TService1, TService2, TService3>()
        {
            return As(new TypedService(typeof(TService1)), new TypedService(typeof(TService2)), new TypedService(typeof(TService3)));
        }

        /// <summary>
        /// Configure the services that the component will provide.
        /// </summary>
        /// <param name="services">Service types to expose.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> As(params Type[] services)
        {
            return As(services.Select(t =>
                t.FullName != null
                ? new TypedService(t)
                : new TypedService(t.GetGenericTypeDefinition()))
                .Cast<Service>().ToArray()); 
        }

        /// <summary>
        /// Configure the services that the component will provide.
        /// </summary>
        /// <param name="services">Services to expose.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> As(params Service[] services)
        {
            if (services == null) throw new ArgumentNullException("services");

            RegistrationData.AddServices(services);

            return this;
        }

        /// <summary>
        /// Provide a textual name that can be used to retrieve the component.
        /// </summary>
        /// <param name="serviceName">Named service to associate with the component.</param>
        /// <param name="serviceType">The service type provided by the component.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> Named(string serviceName, Type serviceType)
        {
            if (serviceName == null) throw new ArgumentNullException("serviceName");
            if (serviceType == null) throw new ArgumentNullException("serviceType");

            return As(new KeyedService(serviceName, serviceType));
        }

        /// <summary>
        /// Provide a textual name that can be used to retrieve the component.
        /// </summary>
        /// <param name="serviceName">Named service to associate with the component.</param>
        /// <typeparam name="TService">The service type provided by the component.</typeparam>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> Named<TService>(string serviceName)
        {
            return Named(serviceName, typeof(TService));
        }

        /// <summary>
        /// Provide a key that can be used to retrieve the component.
        /// </summary>
        /// <param name="serviceKey">Key to associate with the component.</param>
        /// <param name="serviceType">The service type provided by the component.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> Keyed(object serviceKey, Type serviceType)
        {
            if (serviceKey == null) throw new ArgumentNullException("serviceKey");
            if (serviceType == null) throw new ArgumentNullException("serviceType");

            return As(new KeyedService(serviceKey, serviceType));
        }

        /// <summary>
        /// Provide a key that can be used to retrieve the component.
        /// </summary>
        /// <param name="serviceKey">Key to associate with the component.</param>
        /// <typeparam name="TService">The service type provided by the component.</typeparam>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> Keyed<TService>(object serviceKey)
        {
            return Keyed(serviceKey, typeof(TService));
        }

        /// <summary>
        /// Add a handler for the Preparing event. This event allows manipulating of the parameters
        /// that will be provided to the component.
        /// </summary>
        /// <param name="handler">The event handler.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> OnPreparing(Action<PreparingEventArgs> handler)
        {
            if (handler == null) throw new ArgumentNullException("handler");
            RegistrationData.PreparingHandlers.Add((s, e) => handler(e));
            return this;
        }

        /// <summary>
        /// Add a handler for the Activating event.
        /// </summary>
        /// <param name="handler">The event handler.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> OnActivating(Action<IActivatingEventArgs<TLimit>> handler)
        {
            if (handler == null) throw new ArgumentNullException("handler");
            RegistrationData.ActivatingHandlers.Add((s, e) =>
            {
                var args = new ActivatingEventArgs<TLimit>(e.Context, e.Component, e.Parameters, (TLimit)e.Instance);
                handler(args);
                e.Instance = args.Instance;
            });
            return this;
        }

        /// <summary>
        /// Add a handler for the Activated event.
        /// </summary>
        /// <param name="handler">The event handler.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> OnActivated(Action<IActivatedEventArgs<TLimit>> handler)
        {
            if (handler == null) throw new ArgumentNullException("handler");
            RegistrationData.ActivatedHandlers.Add(
                (s, e) => handler(new ActivatedEventArgs<TLimit>(e.Context, e.Component, e.Parameters, (TLimit)e.Instance)));
            return this;
        }

        /// <summary>
        /// Configure the component so that any properties whose types are registered in the
        /// container will be wired to instances of the appropriate service.
        /// </summary>
        /// <param name="wiringFlags">Set wiring options such as circular dependency wiring support.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> PropertiesAutowired(PropertyWiringFlags wiringFlags)
        {
            var injector = new AutowiringPropertyInjector();

            var allowCircularDependencies = 0 != (int)(wiringFlags & PropertyWiringFlags.AllowCircularDependencies);
            var preserveSetValues = 0 != (int) (wiringFlags & PropertyWiringFlags.PreserveSetValues);

            if (allowCircularDependencies)
                RegistrationData.ActivatedHandlers.Add((s, e) => injector.InjectProperties(e.Context, e.Instance, !preserveSetValues));
            else
                RegistrationData.ActivatingHandlers.Add((s, e) => injector.InjectProperties(e.Context, e.Instance, !preserveSetValues));
            
            return this;
        }

        /// <summary>
        /// Associates data with the component.
        /// </summary>
        /// <param name="key">Key by which the data can be located.</param>
        /// <param name="value">The data value.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> WithMetadata(string key, object value)
        {
            if (key == null) throw new ArgumentNullException("key");

            RegistrationData.Metadata.Add(key, value);

            return this;
        }

        /// <summary>
        /// Associates data with the component.
        /// </summary>
        /// <param name="properties">The extended properties to associate with the component.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> WithMetadata(IEnumerable<KeyValuePair<string, object>> properties)
        {
            if (properties == null) throw new ArgumentNullException("properties");

            foreach (var prop in properties)
                WithMetadata(prop.Key, prop.Value);

            return this;
        }

        /// <summary>
        /// Associates data with the component.
        /// </summary>
        /// <typeparam name="TMetadata">A type with properties whose names correspond to the
        /// property names to configure.</typeparam>
        /// <param name="instance">The metadata to associate with the component.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> WithMetadata<TMetadata>(TMetadata instance)
        {
            return WithMetadata(StronglyTypedMetaRegistrationSource.DictionaryKey, instance);
        }
    }
}
