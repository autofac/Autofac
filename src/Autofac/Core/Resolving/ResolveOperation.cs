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

        // _successfulActivations can never be null, but the roslyn compiler doesn't look deeper than
        // the initial constructor methods yet.
        private List<InstanceLookup> _successfulActivations = default!;
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

            // Initialise _successfulActivations.
            ResetSuccessfulActivations();
        }

        /// <inheritdoc />
        public object ResolveComponent(ResolveRequest request)
        {
            return GetOrCreateInstance(_mostNestedLifetimeScope, request);
        }

        /// <summary>
        /// Execute the complete resolve operation.
        /// </summary>
        /// <param name="request">The resolution context.</param>
        [SuppressMessage("CA1031", "CA1031", Justification = "General exception gets rethrown in a DependencyResolutionException.")]
        public object Execute(ResolveRequest request)
        {
            object result;

            try
            {
                result = ResolveComponent(request);
            }
            catch (ObjectDisposedException)
            {
                throw;
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
        /// Continue building the object graph by instantiating <paramref name="request"/> in the
        /// current <paramref name="currentOperationScope"/>.
        /// </summary>
        /// <param name="currentOperationScope">The current scope of the operation.</param>
        /// <param name="request">The resolve request.</param>
        /// <returns>The resolved instance.</returns>
        /// <exception cref="ArgumentNullException"/>
        public object GetOrCreateInstance(ISharingLifetimeScope currentOperationScope, ResolveRequest request)
        {
            if (_ended) throw new ObjectDisposedException(ResolveOperationResources.TemporaryContextDisposed, innerException: null);

            ++_callDepth;

            if (_activationStack.Count > 0)
                CircularDependencyDetector.CheckForCircularDependency(request.Registration, _activationStack, _callDepth);

            var activation = new InstanceLookup(this, currentOperationScope, request);

            _activationStack.Push(activation);

            var handler = InstanceLookupBeginning;
            handler?.Invoke(this, new InstanceLookupBeginningEventArgs(activation));

            try
            {
                var instance = activation.Execute();
                _successfulActivations.Add(activation);

                return instance;
            }
            finally
            {
                // Issue #929: Allow the activation stack to be popped even if the activation failed.
                // This allows try/catch to happen in lambda registrations without corrupting the stack.
                _activationStack.Pop();

                if (_activationStack.Count == 0)
                {
                    CompleteActivations();
                }

                --_callDepth;
            }
        }

        public event EventHandler<ResolveOperationEndingEventArgs>? CurrentOperationEnding;

        public event EventHandler<InstanceLookupBeginningEventArgs>? InstanceLookupBeginning;

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

        private void End(Exception? exception = null)
        {
            if (_ended) return;

            _ended = true;
            var handler = CurrentOperationEnding;
            handler?.Invoke(this, new ResolveOperationEndingEventArgs(this, exception));
        }
    }
}
