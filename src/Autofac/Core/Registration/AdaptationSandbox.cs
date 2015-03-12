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

using System.Collections.Generic;
using System.Linq;
using Autofac.Util;

namespace Autofac.Core.Registration
{
    class AdaptationSandbox
    {
        readonly IEnumerable<IRegistrationSource> _adapters;
        readonly IComponentRegistration _registration;
        readonly IEnumerable<Service> _adapterServices;

        readonly IDictionary<Service, IList<IRegistrationSource>> _adaptersToQuery =
            new Dictionary<Service, IList<IRegistrationSource>>();
        readonly IList<IComponentRegistration> _registrations = new List<IComponentRegistration>();

        public AdaptationSandbox(
            IEnumerable<IRegistrationSource> adapters,
            IComponentRegistration registration,
            IEnumerable<Service> adapterServices)
        {
            _adapters = adapters;
            _registration = registration;
            _adapterServices = adapterServices;
            _registrations.Add(_registration);
        }

        public IEnumerable<IComponentRegistration> GetAdapters()
        {
            foreach (var adapterService in _adapterServices)
                GetAndInitialiseRegistrationsFor(adapterService);

            return _registrations.Where(r => r != _registration);
        }

        IEnumerable<IComponentRegistration> GetAndInitialiseRegistrationsFor(Service service)
        {
            IList<IRegistrationSource> remaining;
            if (!_adaptersToQuery.TryGetValue(service, out remaining))
            {
                remaining = new List<IRegistrationSource>(_adapters);
                _adaptersToQuery.Add(service, remaining);
            }

            foreach (var adapter in _adapters)
            {
                remaining.Remove(adapter);
                var newRegistrations = adapter.RegistrationsFor(service, GetAndInitialiseRegistrationsFor).ToArray();
                _registrations.AddRange(newRegistrations);
            }

            return _registrations.Where(r => r.Services.Contains(service));
        }
    }
}
