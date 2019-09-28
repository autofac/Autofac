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

namespace Autofac.Core
{
    /// <summary>
    /// Describes a logical component within the container.
    /// </summary>
    public interface IComponentRegistration : IDisposable
    {
        /// <summary>
        /// Gets a unique identifier for this component (shared in all sub-contexts.)
        /// This value also appears in Services.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets the activator used to create instances.
        /// </summary>
        IInstanceActivator Activator { get; }

        /// <summary>
        /// Gets the lifetime associated with the component.
        /// </summary>
        IComponentLifetime Lifetime { get; }

        /// <summary>
        /// Gets a value indicating whether the component instances are shared or not.
        /// </summary>
        InstanceSharing Sharing { get; }

        /// <summary>
        /// Gets a value indicating whether the instances of the component should be disposed by the container.
        /// </summary>
        InstanceOwnership Ownership { get; }

        /// <summary>
        /// Gets the services provided by the component.
        /// </summary>
        IEnumerable<Service> Services { get; }

        /// <summary>
        /// Gets additional data associated with the component.
        /// </summary>
        IDictionary<string, object> Metadata { get; }

        /// <summary>
        /// Gets the component registration upon which this registration is based.
        /// </summary>
        IComponentRegistration Target { get; }

        /// <summary>
        /// Gets a value indicating whether the registration is a 1:1 adapter on top
        /// of another component (e.g., Meta, Func, or Owned).
        /// </summary>
        bool IsAdapterForIndividualComponent { get; }

        /// <summary>
        /// Fired when a new instance is required, prior to activation.
        /// Can be used to provide Autofac with additional parameters, used during activation.
        /// </summary>
        event EventHandler<PreparingEventArgs> Preparing;

        /// <summary>
        /// Called by the container when an instance is required.
        /// </summary>
        /// <param name="context">The context in which the instance will be activated.</param>
        /// <param name="parameters">Parameters for activation. These may be modified by the event handler.</param>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "1#", Justification = "The method may change the backing store of the parameter collection.")]
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "This is the method that would raise the event.")]
        void RaisePreparing(IComponentContext context, ref IEnumerable<Parameter> parameters);

        /// <summary>
        /// Fired when a new instance is being activated. The instance can be
        /// wrapped or switched at this time by setting the Instance property in
        /// the provided event arguments.
        /// </summary>
        event EventHandler<ActivatingEventArgs<object>> Activating;

        /// <summary>
        /// Called by the container once an instance has been constructed.
        /// </summary>
        /// <param name="context">The context in which the instance was activated.</param>
        /// <param name="parameters">The parameters supplied to the activator.</param>
        /// <param name="instance">The instance.</param>
        [SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "2#", Justification = "The method may change the object as part of activation.")]
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "This is the method that would raise the event.")]
        void RaiseActivating(IComponentContext context, IEnumerable<Parameter> parameters, ref object instance);

        /// <summary>
        /// Fired when the activation process for a new instance is complete.
        /// </summary>
        event EventHandler<ActivatedEventArgs<object>> Activated;

        /// <summary>
        /// Called by the container once an instance has been fully constructed, including
        /// any requested objects that depend on the instance.
        /// </summary>
        /// <param name="context">The context in which the instance was activated.</param>
        /// <param name="parameters">The parameters supplied to the activator.</param>
        /// <param name="instance">The instance.</param>
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "This is the method that would raise the event.")]
        void RaiseActivated(IComponentContext context, IEnumerable<Parameter> parameters, object instance);
    }
}
