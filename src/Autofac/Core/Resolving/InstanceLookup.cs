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
using System.Globalization;
using System.Linq;
using System.Text;
using Autofac.Builder;
using Autofac.Features.Decorators;

namespace Autofac.Core.Resolving
{
    // Is a component context that pins resolution to a point in the context hierarchy
    [SuppressMessage("Microsoft.ApiDesignGuidelines", "CA2213", Justification = "The instance lookup activation scope gets disposed of by the creator of the scope.")]
    internal class InstanceLookup : IComponentContext, IInstanceLookup
    {
        private readonly IResolveOperation _context;
        private readonly ISharingLifetimeScope _activationScope;
        private readonly IComponentRegistration? _decoratorTargetComponent;
        private readonly Service _service;
        private object? _newInstance;
        private bool _executed;
        private const string ActivatorChainExceptionData = "ActivatorChain";

        public InstanceLookup(
            IResolveOperation context,
            ISharingLifetimeScope mostNestedVisibleScope,
            ResolveRequest request)
        {
            _context = context;
            _service = request.Service;
            _decoratorTargetComponent = request.DecoratorTarget;
            ComponentRegistration = request.Registration;
            Parameters = request.Parameters;

            try
            {
                _activationScope = ComponentRegistration.Lifetime.FindScope(mostNestedVisibleScope);
            }
            catch (DependencyResolutionException ex)
            {
                var services = new StringBuilder();
                foreach (var s in ComponentRegistration.Services)
                {
                    services.Append("- ");
                    services.AppendLine(s.Description);
                }

                var message = string.Format(CultureInfo.CurrentCulture, ComponentActivationResources.UnableToLocateLifetimeScope, ComponentRegistration.Activator.LimitType, services);
                throw new DependencyResolutionException(message, ex);
            }
        }

        public object Execute()
        {
            if (_executed)
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ComponentActivationResources.ActivationAlreadyExecuted, this.ComponentRegistration));

            _executed = true;

            var sharing = _decoratorTargetComponent?.Sharing ?? ComponentRegistration.Sharing;

            var resolveParameters = Parameters as Parameter[] ?? Parameters.ToArray();

            if (!_activationScope.TryGetSharedInstance(ComponentRegistration.Id, out var instance))
            {
                instance = sharing == InstanceSharing.Shared
                    ? _activationScope.CreateSharedInstance(ComponentRegistration.Id, () => CreateInstance(Parameters))
                    : CreateInstance(Parameters);
            }

            var decoratorTarget = instance;

            instance = InstanceDecorator.TryDecorateRegistration(
                _service,
                ComponentRegistration,
                instance,
                _activationScope,
                resolveParameters);

            if (instance != decoratorTarget)
                ComponentRegistration.RaiseActivating(this, resolveParameters, ref instance);

            var handler = InstanceLookupEnding;
            handler?.Invoke(this, new InstanceLookupEndingEventArgs(this, NewInstanceActivated));

            StartStartableComponent(decoratorTarget);

            return instance;
        }

        private void StartStartableComponent(object instance)
        {
            if (instance is IStartable startable
                && ComponentRegistration.Services.Any(s => (s is TypedService typed) && typed.ServiceType == typeof(IStartable))
                && !ComponentRegistration.Metadata.ContainsKey(MetadataKeys.AutoActivated)
                && ComponentRegistry.Properties.ContainsKey(MetadataKeys.StartOnActivatePropertyKey))
            {
                // Issue #916: Set the startable as "done starting" BEFORE calling Start
                // so you don't get a StackOverflow if the component creates a child scope
                // during Start. You don't want the startable trying to activate itself.
                ComponentRegistration.Metadata[MetadataKeys.AutoActivated] = true;
                startable.Start();
            }
        }

        private bool NewInstanceActivated => _newInstance != null;

        [SuppressMessage("CA1031", "CA1031", Justification = "General exception gets rethrown in a PropagateActivationException.")]
        private object CreateInstance(IEnumerable<Parameter> parameters)
        {
            ComponentRegistration.RaisePreparing(this, ref parameters);

            var resolveParameters = parameters as Parameter[] ?? parameters.ToArray();

            try
            {
                _newInstance = ComponentRegistration.Activator.ActivateInstance(this, resolveParameters);

                ComponentRegistration.RaiseActivating(this, resolveParameters, ref _newInstance);
            }
            catch (ObjectDisposedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw PropagateActivationException(this.ComponentRegistration.Activator, ex);
            }

            if (ComponentRegistration.Ownership == InstanceOwnership.OwnedByLifetimeScope)
            {
                // The fact this adds instances for disposal agnostic of the activator is
                // important. The ProvidedInstanceActivator will NOT dispose of the provided
                // instance once the instance has been activated - assuming that it will be
                // done during the lifetime scope's Disposer executing.
                if (_newInstance is IDisposable instanceAsDisposable)
                {
                    _activationScope.Disposer.AddInstanceForDisposal(instanceAsDisposable);
                }
                else if (_newInstance is IAsyncDisposable asyncDisposableInstance)
                {
                    _activationScope.Disposer.AddInstanceForAsyncDisposal(asyncDisposableInstance);
                }
            }

            return _newInstance;
        }

        private static DependencyResolutionException PropagateActivationException(IInstanceActivator activator, Exception exception)
        {
            var activatorChain = activator.DisplayName();
            var innerException = exception;

            if (exception.Data.Contains(ActivatorChainExceptionData) &&
                exception.Data[ActivatorChainExceptionData] is string innerChain)
            {
                activatorChain = activatorChain + " -> " + innerChain;
                innerException = exception.InnerException;
            }

            var result = new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, ComponentActivationResources.ErrorDuringActivation, activatorChain), innerException);
            result.Data[ActivatorChainExceptionData] = activatorChain;
            return result;
        }

        public void Complete()
        {
            if (!NewInstanceActivated) return;

            var beginningHandler = CompletionBeginning;
            beginningHandler?.Invoke(this, new InstanceLookupCompletionBeginningEventArgs(this));

            ComponentRegistration.RaiseActivated(this, Parameters, _newInstance!);

            var endingHandler = CompletionEnding;
            endingHandler?.Invoke(this, new InstanceLookupCompletionEndingEventArgs(this));
        }

        public IComponentRegistry ComponentRegistry => _activationScope.ComponentRegistry;

        public object ResolveComponent(ResolveRequest request)
        {
            return _context.GetOrCreateInstance(_activationScope, request);
        }

        public IComponentRegistration ComponentRegistration { get; }

        public ILifetimeScope ActivationScope => _activationScope;

        public IEnumerable<Parameter> Parameters { get; }

        public event EventHandler<InstanceLookupEndingEventArgs>? InstanceLookupEnding;

        public event EventHandler<InstanceLookupCompletionBeginningEventArgs>? CompletionBeginning;

        public event EventHandler<InstanceLookupCompletionEndingEventArgs>? CompletionEnding;
    }
}
