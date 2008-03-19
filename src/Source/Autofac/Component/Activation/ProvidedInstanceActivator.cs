// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2008 Autofac Contributors
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
using System.Globalization;

namespace Autofac.Component.Activation
{
    /// <summary>
    /// A component activator that returns an already-created instance.
    /// </summary>
    /// <remarks>
    /// The activator only works once. Subsequent calls to ActivateInstance()
    /// will result in a failure (this is to comply with the requirements
    /// of the interface.)
    /// </remarks>
    public class ProvidedInstanceActivator : IActivator
	{
        object _instance;
        object _synchRoot = new object();

        /// <summary>
        /// Create a new InstanceComponentActivator that will
        /// return <paramref name="componentInstance"/> from ActivateInstance().
        /// </summary>
        /// <param name="componentInstance">The instance provided by this activator.</param>
		public ProvidedInstanceActivator(object componentInstance)
		{
            Enforce.ArgumentNotNull(componentInstance, "componentInstance");

			_instance = componentInstance;
		}

        /// <summary>
        /// Create a component instance, using container
        /// to resolve the instance's dependencies.
        /// </summary>
        /// <param name="context">The context to use
        /// for dependency resolution.</param>
        /// <param name="parameters">Unused.</param>
        /// <returns>
        /// A component instance. Note that while the
        /// returned value need not be created on-the-spot, it must
        /// not be returned more than once by consecutive calls. (Throw
        /// an exception if this is attempted. IActivationScope should
        /// manage singleton semantics.)
        /// </returns>
        public object ActivateInstance(IContext context, IActivationParameters parameters)
        {
            Enforce.ArgumentNotNull(context, "context");
            Enforce.ArgumentNotNull(parameters, "parameters");

            lock (_synchRoot)
            {
                if (_instance == null)
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
                        ProvidedInstanceActivatorResources.SingleInstanceLifecycleRequired));

                object result = _instance;
                _instance = null;
                return result;
            }
        }

        /// <summary>
        /// A 'new context' is a scope that is self-contained
        /// and that can dispose the components it contains before the parent
        /// container is disposed. If the activator is stateless it should return
        /// true, otherwise false.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance can support a new context; otherwise, <c>false</c>.
        /// </value>
		public bool CanSupportNewContext
		{
			get
			{
				return false;
			}
		}
	}
}
