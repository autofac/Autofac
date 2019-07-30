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
using System.Diagnostics.CodeAnalysis;
using Autofac.Util;

namespace Autofac.Core.Registration
{
    /// <summary>
    /// Delegates registration lookups to a specified registry. When write operations are applied,
    /// initialises a new 'writeable' registry.
    /// </summary>
    /// <remarks>
    /// Safe for concurrent access by multiple readers. Write operations are single-threaded.
    /// </remarks>
    [SuppressMessage("Microsoft.ApiDesignGuidelines", "CA2213", Justification = "The owner of the createWriteRegistry function is responsible for subsequent disposal of the registry created.")]
    internal class CopyOnWriteRegistry : IComponentRegistry
    {
        private readonly IComponentRegistry _readRegistry;
        private readonly Func<IComponentRegistry> _createWriteRegistry;
        private IComponentRegistry _writeRegistry;

        public CopyOnWriteRegistry(IComponentRegistry readRegistry, Func<IComponentRegistry> createWriteRegistry)
        {
            _readRegistry = readRegistry ?? throw new ArgumentNullException(nameof(readRegistry));
            _createWriteRegistry = createWriteRegistry ?? throw new ArgumentNullException(nameof(createWriteRegistry));
            Properties = new FallbackDictionary<string, object>(readRegistry.Properties);
        }

        private IComponentRegistry Registry => _writeRegistry ?? _readRegistry;

        private IComponentRegistry WriteRegistry => _writeRegistry ?? (_writeRegistry = _createWriteRegistry());

        /// <summary>
        /// Gets or sets the set of properties used during component registration.
        /// </summary>
        /// <value>
        /// An <see cref="IDictionary{TKey, TValue}"/> that can be used to share
        /// context across registrations.
        /// </value>
        public IDictionary<string, object> Properties { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // The _readRegistry doesn't need to be disposed if it still points to the initial registry.
                // Only the potentially allocated registry, containing additional registrations, needs to be disposed.
                _writeRegistry?.Dispose();
            }
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
            add => WriteRegistry.Registered += value;
            remove => WriteRegistry.Registered -= value;
        }

        public void AddRegistrationSource(IRegistrationSource source)
        {
            WriteRegistry.AddRegistrationSource(source);
        }

        public IEnumerable<IRegistrationSource> Sources => Registry.Sources;

        public bool HasLocalComponents => _writeRegistry != null;

        public event EventHandler<RegistrationSourceAddedEventArgs> RegistrationSourceAdded
        {
            add => WriteRegistry.RegistrationSourceAdded += value;
            remove => WriteRegistry.RegistrationSourceAdded -= value;
        }
    }
}