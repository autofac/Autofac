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
using System.Diagnostics.CodeAnalysis;
using Autofac.Core.Registration;

namespace Autofac.Core.Resolving
{
    /// <summary>
    /// A <see cref="ResolveOperation"/> is a component context that sequences and monitors the multiple
    /// activations that go into producing a single requested object graph.
    /// </summary>
    [SuppressMessage("Microsoft.ApiDesignGuidelines", "CA2213", Justification = "The creator of the most nested lifetime scope is responsible for disposal.")]
    internal class ResolveOperation : IComponentContext, IResolveOperation
    {
        private readonly Stack<InstanceLookup> _activationStack = new Stack<InstanceLookup>();
        private List<InstanceLookup> _successfulActivations;
        private readonly ISharingLifetimeScope _mostNestedLifetimeScope;
        private int _callDepth;
        private bool _ended;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolveOperation"/> class.
        /// </summary>
        /// <param name="mostNestedLifetimeScope">The most nested scope in which to begin the operation. The operation
        /// can move upward to less nested scopes as components with wider sharing scopes are activated.</param>
        public ResolveOperation(ISharingLifetimeScope mostNestedLifetimeScope)
        {
            _mostNestedLifetimeScope = mostNestedLifetimeScope;
            ResetSuccessfulActivations();
        }

        /// <summary>
        /// Resolve an instance of the provided registration within the context.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <param name="parameters">Parameters for the instance.</param>
        /// <returns>
        /// The component instance.
        /// </returns>
        /// <exception cref="ComponentNotRegisteredException"/>
        /// <exception cref="DependencyResolutionException"/>
        public object ResolveComponent(IComponentRegistration registration, IEnumerable<Parameter> parameters)
        {
            return GetOrCreateInstance(_mostNestedLifetimeScope, registration, parameters);
        }

        /// <summary>
        /// Execute the complete resolve operation.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <param name="parameters">Parameters for the instance.</param>
        public object Execute(IComponentRegistration registration, IEnumerable<Parameter> parameters)
        {
            object result;

            try
            {
                result = ResolveComponent(registration, parameters);
            }
            catch (DependencyResolutionException dependencyResolutionException)
            {
                End(dependencyResolutionException);
                throw;
            }
            catch (Exception exception)
            {
                End(exception);
                throw new DependencyResolutionException(ResolveOperationResources.ExceptionDuringResolve, exception);
            }

            End();
            return result;
        }

        /// <summary>
        /// Continue building the object graph by instantiating <paramref name="registration"/> in the
        /// current <paramref name="currentOperationScope"/>.
        /// </summary>
        /// <param name="currentOperationScope">The current scope of the operation.</param>
        /// <param name="registration">The component to activate.</param>
        /// <param name="parameters">The parameters for the component.</param>
        /// <returns>The resolved instance.</returns>
        /// <exception cref="ArgumentNullException"/>
        public object GetOrCreateInstance(ISharingLifetimeScope currentOperationScope, IComponentRegistration registration, IEnumerable<Parameter> parameters)
        {
            if (_ended) throw new ObjectDisposedException(ResolveOperationResources.TemporaryContextDisposed, innerException: null);

            ++_callDepth;

            if (_activationStack.Count > 0) CircularDependencyDetector.CheckForCircularDependency(registration, _activationStack, _callDepth);

            var activation = new InstanceLookup(registration, this, currentOperationScope, parameters);

            _activationStack.Push(activation);

            var handler = InstanceLookupBeginning;
            handler?.Invoke(this, new InstanceLookupBeginningEventArgs(activation));

            var instance = activation.Execute();
            _successfulActivations.Add(activation);

            _activationStack.Pop();

            if (_activationStack.Count == 0)
                CompleteActivations();

            --_callDepth;

            return instance;
        }

        public event EventHandler<ResolveOperationEndingEventArgs> CurrentOperationEnding;

        public event EventHandler<InstanceLookupBeginningEventArgs> InstanceLookupBeginning;

        private void CompleteActivations()
        {
            List<InstanceLookup> completed = _successfulActivations;
            int count = completed.Count;
            ResetSuccessfulActivations();

            for (int i = 0; i < count; i++)
            {
                completed[i].Complete();
            }
        }

        private void ResetSuccessfulActivations()
        {
            _successfulActivations = new List<InstanceLookup>();
        }

        /// <summary>
        /// Gets the services associated with the components that provide them.
        /// </summary>
        public IComponentRegistry ComponentRegistry => _mostNestedLifetimeScope.ComponentRegistry;

        private void End(Exception exception = null)
        {
            if (_ended) return;

            _ended = true;
            var handler = CurrentOperationEnding;
            handler?.Invoke(this, new ResolveOperationEndingEventArgs(this, exception));
        }
    }
}
