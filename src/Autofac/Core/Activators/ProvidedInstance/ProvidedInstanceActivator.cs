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
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, ProvidedInstanceActivatorResources.InstanceAlreadyActivated, _instance.GetType()));
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
