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

using System.Collections.Generic;
using Autofac.Util;

namespace Autofac.Core.Resolving
{
    // Is a component context that sequences and monitors the multiple
    // activations that go into producing a single requested object graph
    class ResolveOperation : IComponentContext, IResolveOperation
    {
        Stack<ComponentActivation> _activationStack = new Stack<ComponentActivation>();
        ICollection<ComponentActivation> _successfulActivations;
        ISharingLifetimeScope _mostNestedLifetimeScope;
        CircularDependencyDetector _circularDependencyDetector = new CircularDependencyDetector();
        int _callDepth = 0;

        public ResolveOperation(ISharingLifetimeScope mostNestedLifetimeScope)
        {
            _mostNestedLifetimeScope = Enforce.ArgumentNotNull(mostNestedLifetimeScope, "mostNestedLifetimeScope");
            ResetSuccessfulActivations();
        }

        public object Resolve(IComponentRegistration registration, IEnumerable<Parameter> parameters)
        {
            return Resolve(_mostNestedLifetimeScope, registration, parameters);
        }

        public object Resolve(ISharingLifetimeScope activationScope, IComponentRegistration registration, IEnumerable<Parameter> parameters)
        {
            Enforce.ArgumentNotNull(activationScope, "activationScope");
            Enforce.ArgumentNotNull(registration, "registration");
            Enforce.ArgumentNotNull(parameters, "parameters");

            _circularDependencyDetector.CheckForCircularDependency(registration, _activationStack, ++_callDepth);

            object instance = null;

            var activation = new ComponentActivation(registration, this, activationScope);

            _activationStack.Push(activation);
            try
            {
                instance = activation.Execute(parameters);
                _successfulActivations.Add(activation);
            }
            finally
            {
                _activationStack.Pop();
            }

            if (_activationStack.Count == 0)
                CompleteActivations(parameters);

            --_callDepth;

            return instance;
        }

        void CompleteActivations(IEnumerable<Parameter> parameters)
        {
            var completed = _successfulActivations;
            ResetSuccessfulActivations();

            foreach (var activation in completed)
                activation.Complete(parameters);
        }

        void ResetSuccessfulActivations()
        {
            _successfulActivations = new List<ComponentActivation>();
        }

        public IComponentRegistry ComponentRegistry
        {
            get { return _mostNestedLifetimeScope.ComponentRegistry; }
        }
    }
}
