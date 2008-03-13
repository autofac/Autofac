// This software is part of the Autofac IoC container
// Copyright (c) 2007 Nicholas Blumhardt
// nicholas.blumhardt@gmail.com
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

namespace Autofac.Component.Scope
{
    /// <summary>
    /// Scope model tied to the lifetime of the container in which
    /// the instance is resolved. This is useful for items whose lifespan
    /// is a single web request, for instance.
    /// </summary>
	public class ContainerScope : IScope
	{
        object _instance;

        #region IActivationScope Members

        /// <summary>
        /// Returns true if there is already an instance available
        /// in the current activation scope.
        /// </summary>
        /// <value></value>
        public bool InstanceAvailable
        {
            get
            {
                return _instance != null;
            }
        }

        /// <summary>
        /// The instance corresponding to this scope.
        /// </summary>
        /// <returns>The instance.</returns>
        /// <exception cref="InvalidOperationException">
        /// There is not instance available.</exception>
        public object GetInstance()
        {
            if (_instance == null)
                throw new InvalidOperationException(ContainerScopeResources.InstanceNotAvailable);

            return _instance;
        }

        /// <summary>
        /// Sets the instance to be associated with the
        /// current activation scope.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <exception cref="InvalidOperationException">There is already an instance available.</exception>
        /// <exception cref="ArgumentNullException"></exception>
        public void SetInstance(object instance)
        {
            Enforce.ArgumentNotNull(instance, "instance");

            if (_instance != null)
                throw new InvalidOperationException(ContainerScopeResources.ContextScopeViolated);

                _instance = instance;
        }

        /// <summary>
        /// Try to create a scope container for a new context.
        /// </summary>
        /// <param name="newScope">The duplicate.</param>
        /// <returns>
        /// True if the semantics of the scope model allow for new contexts.
        /// </returns>
		public virtual bool DuplicateForNewContext(out IScope newScope)
		{
			newScope = new ContainerScope();
			return true;
		}

        #endregion
	}
}
