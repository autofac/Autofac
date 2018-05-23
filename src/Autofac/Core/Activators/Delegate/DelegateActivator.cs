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
using System.Globalization;

namespace Autofac.Core.Activators.Delegate
{
    /// <summary>
    /// Activate instances using a delegate.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "There is nothing in the derived class to dispose so no override is necessary.")]
    public class DelegateActivator : InstanceActivator, IInstanceActivator
    {
        private readonly Func<IComponentContext, IEnumerable<Parameter>, object> _activationFunction;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateActivator"/> class.
        /// </summary>
        /// <param name="limitType">The most specific type to which activated instances can be cast.</param>
        /// <param name="activationFunction">Activation delegate.</param>
        public DelegateActivator(Type limitType, Func<IComponentContext, IEnumerable<Parameter>, object> activationFunction)
            : base(limitType)
        {
            if (activationFunction == null) throw new ArgumentNullException(nameof(activationFunction));

            _activationFunction = activationFunction;
        }

        /// <summary>
        /// Activate an instance in the provided context.
        /// </summary>
        /// <param name="context">Context in which to activate instances.</param>
        /// <param name="parameters">Parameters to the instance.</param>
        /// <returns>The activated instance.</returns>
        /// <remarks>
        /// The context parameter here should probably be ILifetimeScope in order to reveal Disposer,
        /// but will wait until implementing a concrete use case to make the decision.
        /// </remarks>
        public object ActivateInstance(IComponentContext context, IEnumerable<Parameter> parameters)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            var result = _activationFunction(context, parameters);
            if (result == null)
            {
                throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, DelegateActivatorResources.NullFromActivationDelegateFor, LimitType));
            }

            return result;
        }
    }
}
