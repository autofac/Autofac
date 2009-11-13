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
using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;

namespace Autofac.Features.OpenGenerics
{
    /// <summary>
    /// Generates activators for open generic types.
    /// </summary>
    public class OpenGenericActivatorGenerator : IActivatorGenerator<ReflectionActivatorData>
    {
        /// <summary>
        /// Given a requested service and registration data, attempt to generate an
        /// activator for the service.
        /// </summary>
        /// <param name="service">Service that was requested.</param>
        /// <param name="configuredServices">Services associated with the activator generator.</param>
        /// <param name="reflectionActivatorData">Data specific to this kind of activator.</param>
        /// <param name="activator">Resulting activator.</param>
        /// <param name="services">Services provided by the activator.</param>
        /// <returns>True if an activator could be generated.</returns>
        public bool TryGenerateActivator(
            Service service,
            IEnumerable<Service> configuredServices,
            ReflectionActivatorData reflectionActivatorData,
            out IInstanceActivator activator,
            out IEnumerable<Service> services)
        {
            Type[] genericParameters = null;
            Type genericTypeDefinition = null;

            var typedService = service as TypedService;
            if (typedService != null)
            {
                var serviceType = typedService.ServiceType;
                if (serviceType.IsGenericType)
                {
                    genericTypeDefinition = serviceType.GetGenericTypeDefinition();
                    genericParameters = serviceType.GetGenericArguments();
                }
            }

            if (genericTypeDefinition != null &&
                configuredServices
                    .DefaultIfEmpty(new TypedService(reflectionActivatorData.ImplementationType))
                    .Cast<TypedService>()
                    .Any(ts => ts.ServiceType == genericTypeDefinition))
            {
                activator = new ReflectionActivator(
                    reflectionActivatorData.ImplementationType.MakeGenericType(genericParameters),
                    reflectionActivatorData.ConstructorFinder,
                    reflectionActivatorData.ConstructorSelector,
                    reflectionActivatorData.ConfiguredParameters,
                    reflectionActivatorData.ConfiguredProperties);

                services = configuredServices
                    .Cast<TypedService>()
                    .Select(ts => new TypedService(ts.ServiceType.MakeGenericType(genericParameters)))
                    .Cast<Service>();

                return true;
            }

            activator = null;
            services = null;
            return false;
        }
    }
}
