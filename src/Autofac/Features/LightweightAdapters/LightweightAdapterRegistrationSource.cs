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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;

namespace Autofac.Features.LightweightAdapters
{
    internal class LightweightAdapterRegistrationSource : IRegistrationSource
    {
        private readonly RegistrationData _registrationData;
        private readonly LightweightAdapterActivatorData _activatorData;

        public LightweightAdapterRegistrationSource(
            RegistrationData registrationData,
            LightweightAdapterActivatorData activatorData)
        {
            if (registrationData == null) throw new ArgumentNullException(nameof(registrationData));
            if (activatorData == null) throw new ArgumentNullException(nameof(activatorData));

            _registrationData = registrationData;
            _activatorData = activatorData;

            if (registrationData.Services.Contains(activatorData.FromService))
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, LightweightAdapterRegistrationSourceResources.FromAndToMustDiffer, activatorData.FromService));
        }

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (registrationAccessor == null) throw new ArgumentNullException(nameof(registrationAccessor));

            if (_registrationData.Services.Contains(service))
            {
                return registrationAccessor(_activatorData.FromService)
                    .Select(r =>
                    {
                        var rb = RegistrationBuilder
                            .ForDelegate((c, p) => _activatorData.Adapter(
                                c, Enumerable.Empty<Parameter>(), c.ResolveComponent(new ResolveRequest(_activatorData.FromService, r, p))))
                            .Targeting(r, IsAdapterForIndividualComponents)
                            .InheritRegistrationOrderFrom(r);

                        rb.RegistrationData.CopyFrom(_registrationData, true);

                        return rb.CreateRegistration();
                    });
            }

            var requestedServiceWithType = service as IServiceWithType;
            var adapteeServiceWithType = _activatorData.FromService as IServiceWithType;

            if (
                //// requested and adaptee are services with type
                //// not including decorators here
                //// and if this registration source contains requested service's type
                (requestedServiceWithType != null && adapteeServiceWithType != null) &&
                (requestedServiceWithType.ServiceType != adapteeServiceWithType.ServiceType) &&
                _registrationData.Services.OfType<IServiceWithType>().Any(s => s.ServiceType == requestedServiceWithType.ServiceType))
            {
                // we try to find registrations for the adaptee service but preserve info from the requested service e.g. keys
                var serviceToFind = requestedServiceWithType.ChangeType(adapteeServiceWithType.ServiceType);

                return registrationAccessor(serviceToFind)
                    .Select(r =>
                    {
                        var rb = RegistrationBuilder
                            .ForDelegate((c, p) => _activatorData.Adapter(
                                c, p, c.ResolveComponent(new ResolveRequest(serviceToFind, r, Enumerable.Empty<Parameter>()))))
                            .Targeting(r, IsAdapterForIndividualComponents);

                        rb.RegistrationData.CopyFrom(_registrationData, true);

                        // we explicitly add requested service to the RegistrationData
                        rb.RegistrationData.AddService(service);

                        return rb.CreateRegistration();
                    });
            }

            return Enumerable.Empty<IComponentRegistration>();
        }

        public bool IsAdapterForIndividualComponents => true;

        public override string ToString()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                LightweightAdapterRegistrationSourceResources.AdapterFromToDescription,
                _activatorData.FromService.Description,
                string.Join(", ", _registrationData.Services.Select(s => s.Description).ToArray()));
        }
    }
}
