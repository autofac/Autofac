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

namespace Autofac.Core.Registration
{
    /// <summary>
    /// Delegates registration lookups to a specified registry. When write operations are applied,
    /// initialises a new 'writeable' registry.
    /// </summary>
    /// <remarks>
    /// Safe for concurrent access by multiple readers. Write operations are single-threaded.
    /// </remarks>
    class CopyOnWriteRegistry : IComponentRegistry
    {
        readonly IComponentRegistry _readRegistry;
        readonly Func<IComponentRegistry> _createWriteRegistry;
        IComponentRegistry _writeRegistry;

        public CopyOnWriteRegistry(IComponentRegistry readRegistry, Func<IComponentRegistry> createWriteRegistry)
        {
            if (readRegistry == null) throw new ArgumentNullException(nameof(readRegistry));
            if (createWriteRegistry == null) throw new ArgumentNullException(nameof(createWriteRegistry));

            _readRegistry = readRegistry;
            _createWriteRegistry = createWriteRegistry;
        }

        IComponentRegistry Registry => _writeRegistry ?? _readRegistry;

        IComponentRegistry WriteRegistry => _writeRegistry ?? (_writeRegistry = _createWriteRegistry());

        public void Dispose()
        {
            _readRegistry?.Dispose();
        }

        public bool TryGetRegistration(Service service, out IComponentRegistration registration)
        {
            return Registry.TryGetRegistration(service, out registration);
        }

        public bool IsRegistered(Service service)
        {
            return Registry.IsRegistered(service);
        }

        public void Register(IComponentRegistration registration)
        {
            WriteRegistry.Register(registration);
        }

        public void Register(IComponentRegistration registration, bool preserveDefaults)
        {
            WriteRegistry.Register(registration, preserveDefaults);
        }

        public IEnumerable<IComponentRegistration> Registrations => Registry.Registrations;

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service)
        {
            return Registry.RegistrationsFor(service);
        }

        public event EventHandler<ComponentRegisteredEventArgs> Registered
        {
            add { WriteRegistry.Registered += value; }
            remove { WriteRegistry.Registered -= value; }
        }

        public void AddRegistrationSource(IRegistrationSource source)
        {
            WriteRegistry.AddRegistrationSource(source);
        }

        public IEnumerable<IRegistrationSource> Sources => Registry.Sources;

        public bool HasLocalComponents => _writeRegistry != null;

        public event EventHandler<RegistrationSourceAddedEventArgs> RegistrationSourceAdded
        {
            add { WriteRegistry.RegistrationSourceAdded += value; }
            remove { WriteRegistry.RegistrationSourceAdded -= value; }
        }
    }
}