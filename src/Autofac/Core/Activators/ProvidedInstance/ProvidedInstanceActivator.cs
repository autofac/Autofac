// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Autofac.Core.Resolving;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Activators.ProvidedInstance
{
    /// <summary>
    /// Provides a pre-constructed instance.
    /// </summary>
    public class ProvidedInstanceActivator : InstanceActivator, IInstanceActivator
    {
        private readonly object _instance;
        private bool _activated;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProvidedInstanceActivator"/> class.
        /// </summary>
        /// <param name="instance">The instance to provide.</param>
        public ProvidedInstanceActivator(object instance)
            : base(GetType(instance))
        {
            _instance = instance;
        }

        /// <inheritdoc/>
        public void ConfigurePipeline(IComponentRegistryServices componentRegistryServices, IResolvePipelineBuilder pipelineBuilder)
        {
            if (pipelineBuilder is null)
            {
                throw new ArgumentNullException(nameof(pipelineBuilder));
            }

            pipelineBuilder.Use(this.DisplayName(), PipelinePhase.Activation, MiddlewareInsertionMode.EndOfPhase, (ctxt, next) =>
            {
                ctxt.Instance = GetInstance();

                next(ctxt);
            });
        }

        private object GetInstance()
        {
            CheckNotDisposed();

            if (_activated)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ProvidedInstanceActivatorResources.InstanceAlreadyActivated, _instance.GetType()));
            }

            _activated = true;

            return _instance;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the activator disposes the instance that it holds.
        /// Necessary because otherwise instances that are never resolved will never be
        /// disposed.
        /// </summary>
        public bool DisposeInstance { get; set; }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            // Only dispose of the instance here if it wasn't activated. If it was activated,
            // then either the owning lifetime scope will dispose of it automatically
            // (see InstanceLookup.Activate) or an OnRelease handler will take care of it.
            if (disposing && DisposeInstance && _instance is IDisposable disposable && !_activated)
            {
                disposable.Dispose();
            }

            base.Dispose(disposing);
        }

        [SuppressMessage("CA2222", "CA2222", Justification = "False positive. GetType doesn't hide an inherited member.")]
        private static Type GetType(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            return instance.GetType();
        }
    }
}
