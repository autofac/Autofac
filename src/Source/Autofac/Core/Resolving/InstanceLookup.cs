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
    class InstanceLookup : IComponentContext, IInstanceLookup
    {
        readonly IEnumerable<Parameter> _parameters;
        readonly IComponentRegistration _componentRegistration;
        readonly IResolveOperation _context;
        readonly ISharingLifetimeScope _activationScope;
        object _newInstance;
        bool _executed;

        public InstanceLookup(
            IComponentRegistration registration,
            IResolveOperation context,
            ISharingLifetimeScope mostNestedVisibleScope,
            IEnumerable<Parameter> parameters)
        {
            _parameters = parameters;
            _componentRegistration = Enforce.ArgumentNotNull(registration, "registration");
            _context = Enforce.ArgumentNotNull(context, "context");
            if (mostNestedVisibleScope == null) throw new ArgumentNullException("mostNestedVisibleScope");
            if (parameters == null) throw new ArgumentNullException("parameters");
            _activationScope = _componentRegistration.Lifetime.FindScope(mostNestedVisibleScope);
        }

        public object Execute()
        {
            if (_executed)
                throw new InvalidOperationException(ComponentActivationResources.ActivationAlreadyExecuted);
            
            _executed = true;

            object instance;
            if (_componentRegistration.Sharing == InstanceSharing.None)
                instance = Activate(Parameters);
            else
                instance = _activationScope.GetOrCreateAndShare(_componentRegistration.Id, () => Activate(Parameters));

            InstanceLookupEnding(this, new InstanceLookupEndingEventArgs(this, NewInstanceActivated));

            return instance;
        }

        bool NewInstanceActivated { get { return _newInstance != null; } }

        object Activate(IEnumerable<Parameter> parameters)
        {
            _componentRegistration.RaisePreparing(this, ref parameters);

            _newInstance = _componentRegistration.Activator.ActivateInstance(this, parameters);

            if (_componentRegistration.Ownership == InstanceOwnership.OwnedByLifetimeScope)
            {
                var instanceAsDisposable = _newInstance as IDisposable;
                if (instanceAsDisposable != null)
                    _activationScope.Disposer.AddInstanceForDisposal(instanceAsDisposable);
            }

            _componentRegistration.RaiseActivating(this, parameters, ref _newInstance);

            return _newInstance;
        }

        public void Complete()
        {
            if (NewInstanceActivated)
            {
                CompletionBeginning(this, new InstanceLookupCompletionBeginningEventArgs(this));

                _componentRegistration.RaiseActivated(this, Parameters, _newInstance);

                CompletionEnding(this, new InstanceLookupCompletionEndingEventArgs(this));
            }
        }

        public IComponentRegistry ComponentRegistry
        {
            get { return _activationScope.ComponentRegistry; }
        }

        public object ResolveComponent(IComponentRegistration registration, IEnumerable<Parameter> parameters)
        {
            return _context.GetOrCreateInstance(_activationScope, registration, parameters);
        }

        public IComponentRegistration ComponentRegistration { get { return _componentRegistration; } }

        public ILifetimeScope ActivationScope { get { return _activationScope; } }

        public IEnumerable<Parameter> Parameters { get { return _parameters; } }

        public event EventHandler<InstanceLookupEndingEventArgs> InstanceLookupEnding = delegate { };

        public event EventHandler<InstanceLookupCompletionBeginningEventArgs> CompletionBeginning = delegate { };

        public event EventHandler<InstanceLookupCompletionEndingEventArgs> CompletionEnding = delegate { };
    }
}
