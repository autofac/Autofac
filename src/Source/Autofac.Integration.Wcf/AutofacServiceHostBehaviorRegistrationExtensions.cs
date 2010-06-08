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
using System.Collections.Generic;
using System.ServiceModel.Description;
using Autofac.Builder;

namespace Autofac.Integration.Wcf
{
    /// <summary>
    /// Extension methods that help in registering behaviors that should be added
    /// to service hosts when connections are opened.
    /// </summary>
    public static class AutofacServiceHostBehaviorRegistrationExtensions
    {
        /// <summary>
        /// Tag that names the collection of behaviors to add to a service host.
        /// </summary>
        private const string WcfServiceBehaviorForHostCollectionTag = "__wcfServiceHostBehaviors";

        /// <summary>
        /// Registers a behavior to be added to the service host during the
        /// <see cref="System.ServiceModel.ICommunicationObject.Opening"/> event.
        /// </summary>
        /// <typeparam name="TBehavior">
        /// The type of behavior that implements <see cref="System.ServiceModel.Description.IServiceBehavior"/>
        /// to add to the service host.
        /// </typeparam>
        /// <param name="builder">
        /// The <see cref="Autofac.ContainerBuilder"/> in which behaviors should
        /// be registered. This <see cref="Autofac.ContainerBuilder"/> should
        /// build the application container used by the service host factory
        /// <see cref="Autofac.Integration.Wcf.AutofacHostFactory.ContainerProvider"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="builder" /> is <see langword="null" />.
        /// </exception>
        public static IRegistrationBuilder<TBehavior, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterServiceBehaviorForHost<TBehavior>(this ContainerBuilder builder) where TBehavior : IServiceBehavior
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            builder.RegisterCollection<IServiceBehavior>(WcfServiceBehaviorForHostCollectionTag).Named<IEnumerable<IServiceBehavior>>(WcfServiceBehaviorForHostCollectionTag);
            return builder.RegisterType<TBehavior>().MemberOf(WcfServiceBehaviorForHostCollectionTag);
        }

        /// <summary>
        /// Resolves the set of service host behaviors to add during the
        /// <see cref="System.ServiceModel.ICommunicationObject.Opening"/> event.
        /// </summary>
        /// <param name="applicationContainer">
        /// The application container from the
        /// <see cref="Autofac.Integration.Wcf.AutofacHostFactory.ContainerProvider"/>
        /// in which service behaviors were registered using
        /// <see cref="Autofac.Integration.Wcf.AutofacServiceHostBehaviorRegistrationExtensions.RegisterServiceBehaviorForHost"/>.
        /// </param>
        /// <returns>
        /// An <see cref="System.Collections.Generic.IEnumerable{T}"/> of <see cref="System.ServiceModel.Description.IServiceBehavior"/>
        /// with the additional behaviors to add to the service host.
        /// </returns>
        public static IEnumerable<IServiceBehavior> ResolveServiceBehaviorsForHost(this IContainer applicationContainer)
        {
            if (applicationContainer == null)
            {
                throw new ArgumentNullException("applicationContainer");
            }
            return applicationContainer.Resolve<IEnumerable<IServiceBehavior>>(WcfServiceBehaviorForHostCollectionTag);
        }
    }
}
