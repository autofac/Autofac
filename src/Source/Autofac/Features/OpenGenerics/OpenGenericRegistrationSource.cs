// This software is part of the Autofac IoC container
// Copyright (c) 2010 Autofac Contributors
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
using Autofac.Util;

namespace Autofac.Features.OpenGenerics
{
    /// <summary>
    /// Generates activators for open generic types.
    /// </summary>
    class OpenGenericRegistrationSource : IRegistrationSource
    {
        readonly RegistrationData _registrationData;
        readonly ReflectionActivatorData _activatorData;

        public OpenGenericRegistrationSource(
            RegistrationData registrationData,
            ReflectionActivatorData activatorData)
        {
            if (registrationData == null) throw new ArgumentNullException("registrationData");
            if (activatorData == null) throw new ArgumentNullException("activatorData");

            _registrationData = registrationData;
            _activatorData = activatorData;
        }

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            Enforce.ArgumentNotNull(service, "service");
            var result = Enumerable.Empty<IComponentRegistration>();

            IInstanceActivator activator;
            IEnumerable<Service> services;
            if (TryGenerateActivator(service, _registrationData.Services, _activatorData, out activator, out services))
            {
                result = new[] {
                    RegistrationBuilder.CreateRegistration(
                        Guid.NewGuid(),
                        _registrationData,
                        activator,
                        services)
                };
            }

            return result;
        }

        static bool TryGenerateActivator(
            Service service,
            IEnumerable<Service> configuredServices,
            ReflectionActivatorData reflectionActivatorData,
            out IInstanceActivator activator,
            out IEnumerable<Service> services)
        {
            var swt = service as IServiceWithType;
            if (swt != null && swt.ServiceType.IsGenericType)
            {
                var genericTypeDefinition = swt.ServiceType.GetGenericTypeDefinition();
                var genericArguments = swt.ServiceType.GetGenericArguments();

                if (configuredServices
                    .Cast<IServiceWithType>()
                    .Any(s => s.ServiceType == genericTypeDefinition) &&
                    reflectionActivatorData.ImplementationType.IsCompatibleWithGenericParameters(genericArguments))
                {
                    activator = new ReflectionActivator(
                        reflectionActivatorData.ImplementationType.MakeGenericType(genericArguments),
                        reflectionActivatorData.ConstructorFinder,
                        reflectionActivatorData.ConstructorSelector,
                        reflectionActivatorData.ConfiguredParameters,
                        reflectionActivatorData.ConfiguredProperties);

                    services = configuredServices
                        .Cast<IServiceWithType>()
                        .Select(s => s.ChangeType(s.ServiceType.MakeGenericType(genericArguments)));

                    return true;
                }
            }

            activator = null;
            services = null;
            return false;
        }
    }
}
