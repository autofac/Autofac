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
using Autofac.Util;

namespace Autofac.Core.Resolving
{
    // Is a component context that pins resolution to a point in the context hierarchy
    class ComponentActivation : IComponentContext
    {
        readonly IComponentRegistration _registration;
        readonly IResolveOperation _context;
        readonly ISharingLifetimeScope _activationScope;
        object _newInstance;
        bool _executed;

        public ComponentActivation(
            IComponentRegistration registration,
            IResolveOperation context,
            ISharingLifetimeScope mostNestedVisibleScope)
        {
            _registration = Enforce.ArgumentNotNull(registration, "registration");
            _context = Enforce.ArgumentNotNull(context, "context");
            Enforce.ArgumentNotNull(mostNestedVisibleScope, "mostNestedVisibleScope");
            _activationScope = _registration.Lifetime.FindScope(mostNestedVisibleScope);
        }

        public IComponentRegistration Registration { get { return _registration; } }

        public object Execute(IEnumerable<Parameter> parameters)
        {
            Enforce.ArgumentNotNull(parameters, "parameters");
            if (_executed)
                throw new InvalidOperationException("Execute already called.");
            
            _executed = true;

            if (_registration.Sharing == InstanceSharing.None)
                return Activate(parameters);

            return _activationScope.GetOrCreateAndShare(_registration.Id, () => Activate(parameters));
        }

        object Activate(IEnumerable<Parameter> parameters)
        {
            _registration.RaisePreparing(this, ref parameters);

            _newInstance = _registration.Activator.ActivateInstance(this, parameters);

            if (_registration.Ownership == InstanceOwnership.OwnedByLifetimeScope)
            {
                var instanceAsDisposable = _newInstance as IDisposable;
                if (instanceAsDisposable != null)
                    _activationScope.Disposer.AddInstanceForDisposal(instanceAsDisposable);
            }

            _registration.RaiseActivating(this, parameters, ref _newInstance);

            return _newInstance;
        }

        public void Complete(IEnumerable<Parameter> parameters)
        {
            if (_newInstance != null)
                _registration.RaiseActivated(this, parameters, _newInstance);
        }

        public IComponentRegistry ComponentRegistry
        {
            get { return _activationScope.ComponentRegistry; }
        }

        public object ResolveComponent(IComponentRegistration registration, IEnumerable<Parameter> parameters)
        {
            return _context.Resolve(_activationScope, registration, parameters);
        }
    }
}
