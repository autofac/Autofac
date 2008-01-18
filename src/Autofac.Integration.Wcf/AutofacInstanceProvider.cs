// Contributed 2008-01-15 by Petar Andrijasevic
// Copyright (c) 2008 Autofac Contributors
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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace Autofac.Integration.Wcf
{
    /// <summary>
    /// Retrieves service instances from an Autofac container.
    /// </summary>
	public class AutofacInstanceProvider : IInstanceProvider
	{
		private readonly Container _container;
		private readonly Type _type;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacInstanceProvider"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="type">The type to resolve from the container.</param>
        public AutofacInstanceProvider(Container container, Type type)
		{
            if (container == null)
                throw new ArgumentNullException("container");

            if (type == null)
                throw new ArgumentNullException("type");

            this._container = container;
			this._type = type;
		}

		#region IInstanceProvider Members

        /// <summary>
        /// Returns a service object given the specified <see cref="T:System.ServiceModel.InstanceContext"/> object.
        /// </summary>
        /// <param name="instanceContext">The current <see cref="T:System.ServiceModel.InstanceContext"/> object.</param>
        /// <returns>A user-defined service object.</returns>
		public object GetInstance(InstanceContext instanceContext)
		{
			return GetInstance(instanceContext, null);
		}

        /// <summary>
        /// Returns a service object given the specified <see cref="T:System.ServiceModel.InstanceContext"/> object.
        /// </summary>
        /// <param name="instanceContext">The current <see cref="T:System.ServiceModel.InstanceContext"/> object.</param>
        /// <param name="message">The message that triggered the creation of a service object.</param>
        /// <returns>The service object.</returns>
		public object GetInstance(InstanceContext instanceContext, Message message)
		{
            return _container.Resolve(_type);
		}

        /// <summary>
        /// Called when an <see cref="T:System.ServiceModel.InstanceContext"/> object recycles a service object.
        /// </summary>
        /// <param name="instanceContext">The service's instance context.</param>
        /// <param name="instance">The service object to be recycled.</param>
		public void ReleaseInstance(InstanceContext instanceContext, object instance)
		{
			// TODO: This one could be used to make disposal (maybe with an inner container ??)...
		}

		#endregion
	}
}