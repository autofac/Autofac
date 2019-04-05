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
using Autofac.Core.Lifetime;
using Autofac.Util;

namespace Autofac.Core.Registration
{
    /// <summary>
    /// Switches components with a RootScopeLifetime (singletons) with
    /// decorators exposing MatchingScopeLifetime targeting the specified scope.
    /// </summary>
    internal class ScopeRestrictedRegistryBuilder : Disposable, IComponentRegistryBuilder
    {
        private readonly MatchingScopeLifetime _restrictedRootScopeLifetime;
        private readonly IComponentRegistryBuilder _inner;

        internal ScopeRestrictedRegistryBuilder(object scopeTag, IDictionary<string, object> properties)
        {
            _restrictedRootScopeLifetime = new MatchingScopeLifetime(scopeTag);
            var tracker = new ScopeRestrictedRegisteredServicesTracker(_restrictedRootScopeLifetime);

            _inner = new ComponentRegistry(tracker, properties);
        }

        protected override void Dispose(bool disposing)
        {
            _inner.Dispose();
        }

        public IComponentRegistry Build()
        {
            return _inner.Build();
        }

        public IDictionary<string, object> Properties
        {
            get { return _inner.Properties; }
        }

        public void Register(IComponentRegistration registration)
        {
            Register(registration, false);
        }

        public void Register(IComponentRegistration registration, bool preserveDefaults)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            var toRegister = registration;

            if (registration.Lifetime is RootScopeLifetime)
                toRegister = new ComponentRegistrationLifetimeDecorator(registration, _restrictedRootScopeLifetime);

            _inner.Register(toRegister, preserveDefaults);
        }

        public event EventHandler<ComponentRegisteredEventArgs> Registered
        {
            add { _inner.Registered += value; }
            remove { _inner.Registered -= value; }
        }

        public bool IsRegistered(Service service)
        {
            return _inner.IsRegistered(service);
        }

        public void AddRegistrationSource(IRegistrationSource source)
        {
            _inner.AddRegistrationSource(source);
        }

        public event EventHandler<RegistrationSourceAddedEventArgs> RegistrationSourceAdded
        {
            add { _inner.RegistrationSourceAdded += value; }
            remove { _inner.RegistrationSourceAdded -= value; }
        }
    }
}
