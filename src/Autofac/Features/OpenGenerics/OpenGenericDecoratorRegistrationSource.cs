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
using System.Globalization;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;

namespace Autofac.Features.OpenGenerics
{
    internal class OpenGenericDecoratorRegistrationSource : IRegistrationSource
    {
        private readonly RegistrationData _registrationData;
        private readonly OpenGenericDecoratorActivatorData _activatorData;

        public OpenGenericDecoratorRegistrationSource(
            RegistrationData registrationData,
            OpenGenericDecoratorActivatorData activatorData)
        {
            if (registrationData == null) throw new ArgumentNullException(nameof(registrationData));
            if (activatorData == null) throw new ArgumentNullException(nameof(activatorData));

            OpenGenericServiceBinder.EnforceBindable(activatorData.ImplementationType, registrationData.Services);

            if (registrationData.Services.Contains((Service)activatorData.FromService))
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, OpenGenericDecoratorRegistrationSourceResources.FromAndToMustDiffer, activatorData.FromService));

            _registrationData = registrationData;
            _activatorData = activatorData;
        }

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (registrationAccessor == null) throw new ArgumentNullException(nameof(registrationAccessor));

            Type constructedImplementationType;
            Service[] services;
            if (OpenGenericServiceBinder.TryBindServiceType(service, _registrationData.Services, _activatorData.ImplementationType, out constructedImplementationType, out services))
            {
                var swt = (IServiceWithType)service;
                var fromService = _activatorData.FromService.ChangeType(swt.ServiceType);

                return registrationAccessor(fromService)
                    .Select(cr => RegistrationBuilder.CreateRegistration(
                            Guid.NewGuid(),
                            _registrationData,
                            new ReflectionActivator(constructedImplementationType, _activatorData.ConstructorFinder, _activatorData.ConstructorSelector, AddDecoratedComponentParameter(swt.ServiceType, cr, _activatorData.ConfiguredParameters), _activatorData.ConfiguredProperties),
                            services));
            }

            return Enumerable.Empty<IComponentRegistration>();
        }

        private static Parameter[] AddDecoratedComponentParameter(Type decoratedParameterType, IComponentRegistration decoratedComponent, IList<Parameter> configuredParameters)
        {
            var parameter = new ResolvedParameter(
                (pi, c) => pi.ParameterType == decoratedParameterType,
                (pi, c) => c.ResolveComponent(decoratedComponent, Enumerable.Empty<Parameter>()));

            var resultArray = new Parameter[configuredParameters.Count + 1];
            resultArray[0] = parameter;
            for (int i = 0; i < configuredParameters.Count; i++)
            {
                resultArray[i + 1] = configuredParameters[i];
            }

            return resultArray;
        }

        public bool IsAdapterForIndividualComponents => true;

        public override string ToString()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                OpenGenericDecoratorRegistrationSourceResources.OpenGenericDecoratorRegistrationSourceImplFromTo,
                _activatorData.ImplementationType.FullName,
                ((Service)_activatorData.FromService).Description,
                string.Join(", ", _registrationData.Services.Select(s => s.Description).ToArray()));
        }
    }
}
