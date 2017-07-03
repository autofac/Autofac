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
using Autofac.Core.Lifetime;

namespace Autofac.Core.Registration
{
    /// <summary>
    /// Switches components with a RootScopeLifetime (singletons) with
    /// decorators exposing MatchingScopeLifetime targeting the specified scope.
    /// </summary>
    internal class ScopeRestrictedRegistry : ComponentRegistry
    {
        private readonly IComponentLifetime _restrictedRootScopeLifetime;

        internal ScopeRestrictedRegistry(object scopeTag, IDictionary<string, object> properties)
            : base(properties)
        {
            _restrictedRootScopeLifetime = new MatchingScopeLifetime(scopeTag);
        }

        protected override void AddRegistration(IComponentRegistration registration, bool preserveDefaults, bool originatedFromSource = false)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            var toRegister = registration;

            if (registration.Lifetime is RootScopeLifetime)
                toRegister = new ComponentRegistrationLifetimeDecorator(registration, _restrictedRootScopeLifetime);

            base.AddRegistration(toRegister, preserveDefaults, originatedFromSource);
        }
    }
}
