// This software is part of the Autofac IoC container
// Copyright (c) 2012 Autofac Contributors
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
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Metadata;
using System.Web.Http.ModelBinding;
using System.Web.Http.Validation;
using System.Web.Http.ValueProviders;
using Autofac.Features.Metadata;

namespace Autofac.Integration.WebApi
{
    /// <summary>
    /// Configures the controller descriptor with per-controller services from the container.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class AutofacControllerConfigurationAttribute : Attribute, IControllerConfiguration
    {
        const string InitializedKey = "InjectControllerServicesAttributeInitialized";

        internal static readonly string ClearServiceListKey = "ClearServiceList";

        /// <summary>
        /// Callback invoked to set per-controller overrides for this controllerDescriptor.
        /// </summary>
        /// <param name="controllerSettings">The controller settings to initialize.</param>
        /// <param name="controllerDescriptor">The controller descriptor. Note that the 
        /// <see cref="T:System.Web.Http.Controllers.HttpControllerDescriptor"/> can be 
        /// associated with the derived controller type given that <see cref="T:System.Web.Http.Controllers.IControllerConfiguration"/> 
        /// is inherited.</param>
        public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            if (controllerDescriptor.Configuration == null) return;
            if (!controllerDescriptor.Properties.TryAdd(InitializedKey, null)) return;

            var container = controllerDescriptor.Configuration.DependencyResolver.GetRootLifetimeScope();
            if (container == null)
                throw new InvalidOperationException(
                    string.Format(AutofacControllerConfigurationAttributeResources.DependencyResolverMissing,
                        typeof(AutofacWebApiDependencyResolver).Name, typeof(AutofacControllerConfigurationAttribute).Name));

            var controllerServices = controllerSettings.Services;
            var serviceKey = new ControllerTypeKey(controllerDescriptor.ControllerType);

            UpdateControllerService<IHttpActionInvoker>(controllerServices, container, serviceKey);
            UpdateControllerService<IHttpActionSelector>(controllerServices, container, serviceKey);
            UpdateControllerService<IActionValueBinder>(controllerServices, container, serviceKey);
            UpdateControllerService<IBodyModelValidator>(controllerServices, container, serviceKey);
            UpdateControllerService<IContentNegotiator>(controllerServices, container, serviceKey);
            UpdateControllerService<IHttpControllerActivator>(controllerServices, container, serviceKey);
            UpdateControllerService<ModelMetadataProvider>(controllerServices, container, serviceKey);

            UpdateControllerServices<ModelBinderProvider>(controllerServices, container, serviceKey);
            UpdateControllerServices<ModelValidatorProvider>(controllerServices, container, serviceKey);
            UpdateControllerServices<ValueProviderFactory>(controllerServices, container, serviceKey);

            UpdateControllerFormatters(controllerSettings.Formatters, container, serviceKey);
        }

        static void UpdateControllerService<T>(ServicesContainer services, IComponentContext container, ControllerTypeKey serviceKey) where T : class
        {
            var instance = container.ResolveOptionalKeyed<Meta<T>>(serviceKey);
            if (instance != null)
                services.Replace(typeof(T), instance.Value);
        }

        static void UpdateControllerServices<T>(ServicesContainer services, IComponentContext container, ControllerTypeKey serviceKey) where T : class
        {
            var resolvedInstances = container.ResolveOptionalKeyed<IEnumerable<Meta<T>>>(serviceKey).ToArray();

            if (resolvedInstances.Any(service => ClearExistingServices(service.Metadata)))
                services.Clear(typeof(T));

            foreach (var instance in resolvedInstances)
                services.Add(typeof(T), instance.Value);
        }

        static void UpdateControllerFormatters(ICollection<MediaTypeFormatter> collection, IComponentContext container, ControllerTypeKey serviceKey)
        {
            var formatters = container.ResolveOptionalKeyed<IEnumerable<Meta<MediaTypeFormatter>>>(serviceKey).ToArray();

            if (formatters.Any(service => ClearExistingServices(service.Metadata)))
                collection.Clear();

            foreach (var formatter in formatters)
                collection.Add(formatter.Value);
        }

        static bool ClearExistingServices(IDictionary<string, object> metadata)
        {
            return metadata.ContainsKey(ClearServiceListKey) && (bool)metadata[ClearServiceListKey];
        }
    }
}