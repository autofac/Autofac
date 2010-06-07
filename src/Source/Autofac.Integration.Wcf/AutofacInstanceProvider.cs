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

using System;
using System.Globalization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace Autofac.Integration.Wcf
{
    /// <summary>
    /// Retrieves service instances from the application container provider.
    /// </summary>
    public class AutofacInstanceProvider : IInstanceProvider
    {
        private readonly ServiceImplementationData _serviceImplementationData;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacInstanceProvider"/> class.
        /// </summary>
        /// <param name="serviceImplementationData">
        /// Data about the service being hosted and how to resolve the component
        /// registration from the request lifetime.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="serviceImplementationData" /> is <see langword="null" />.
        /// </exception>
        public AutofacInstanceProvider(ServiceImplementationData serviceImplementationData)
        {
            if (serviceImplementationData == null)
                throw new ArgumentNullException("serviceImplementationData");

            this._serviceImplementationData = serviceImplementationData;
        }

        #region IInstanceProvider Members

        /// <summary>
        /// Returns a service object given the specified <see cref="T:System.ServiceModel.InstanceContext"/> object.
        /// </summary>
        /// <param name="instanceContext">The current <see cref="T:System.ServiceModel.InstanceContext"/> object.</param>
        /// <returns>A user-defined service object.</returns>
        public virtual object GetInstance(InstanceContext instanceContext)
        {
            return GetInstance(instanceContext, null);
        }

        /// <summary>
        /// Returns a service object given the specified <see cref="T:System.ServiceModel.InstanceContext"/> object.
        /// </summary>
        /// <param name="instanceContext">The current <see cref="T:System.ServiceModel.InstanceContext"/> object.</param>
        /// <param name="message">The message that triggered the creation of a service object.</param>
        /// <returns>The service object.</returns>
        /// <exception cref="System.InvalidOperationException">
        /// <para>
        /// Thrown if:
        /// </para>
        /// <para>
        /// The <see cref="Autofac.Integration.Wcf.ServiceImplementationData.ImplementationResolver"/>
        /// is <see langword="null" /> on the <see cref="Autofac.Integration.Wcf.ServiceImplementationData"/>
        /// object that was passed in during construction.
        /// </para>
        /// <para>
        /// OR
        /// </para>
        /// <para>
        /// The object returned from the <see cref="Autofac.Integration.Wcf.ServiceImplementationData.ImplementationResolver"/>
        /// is <see langword="null" />.
        /// </para>
        /// </exception>
        public virtual object GetInstance(InstanceContext instanceContext, Message message)
        {
            if (this._serviceImplementationData.ImplementationResolver == null)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AutofacServiceHostFactoryResources.ImplementationTypeResolverIsNull, this._serviceImplementationData.ConstructorString));
            }
            var implementation = this._serviceImplementationData.ImplementationResolver(AutofacHostFactory.ContainerProvider.RequestLifetime);
            if (implementation == null)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AutofacServiceHostFactoryResources.ServiceNotRegistered, this._serviceImplementationData.ConstructorString));
            }

            return implementation;
        }

        /// <summary>
        /// Called when an <see cref="T:System.ServiceModel.InstanceContext"/> object recycles a service object.
        /// </summary>
        /// <param name="instanceContext">The service's instance context.</param>
        /// <param name="instance">The service object to be recycled.</param>
        public virtual void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            AutofacHostFactory.ContainerProvider.EndRequestLifetime();
        }

        #endregion
    }
}