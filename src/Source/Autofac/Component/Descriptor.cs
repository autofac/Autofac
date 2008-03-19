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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Component
{
    /// <summary>
    /// Describes a component and its services.
    /// </summary>
    public class Descriptor : IComponentDescriptor
    {
        Type _knownImplementationType;

        /// <summary>
        /// Initializes a new instance of the <see cref="Descriptor"/> class.
        /// </summary>
        /// <param name="id">The id. Will be exposed as a service.</param>
        /// <param name="services">The services. May or may not contain the id.</param>
        /// <param name="bestKnownImplementationType">Most specific type that can be
        /// determined as the implementation.</param>
        public Descriptor(
            Service id,
            IEnumerable<Service> services,
            Type bestKnownImplementationType)
        : this(id, services, bestKnownImplementationType, new Dictionary<string, object>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Descriptor"/> class.
        /// </summary>
        /// <param name="id">The id. Will be exposed as a service.</param>
        /// <param name="services">The services. May or may not contain the id.</param>
        /// <param name="extendedProperties">The extended properties.</param>
        /// <param name="bestKnownImplementationType">Most specific type that can be
        /// determined as the implementation.</param>
        public Descriptor(
            Service id, 
            IEnumerable<Service> services, 
            Type bestKnownImplementationType,
            IDictionary<string, object> extendedProperties)
        {
            Id = Enforce.ArgumentNotNull(id, "id");
            
            Services = new[] { id }
                .Concat(Enforce.ArgumentNotNull(services, "services"))
                .Distinct();

            if (Services.Contains(null))
                throw new ArgumentException(DescriptorResources.NullServiceProvided);
            
            ExtendedProperties = new Dictionary<string, object>(
                Enforce.ArgumentNotNull(extendedProperties, "extendedProperties"));

            Enforce.ArgumentNotNull(bestKnownImplementationType, "bestKnownImplementationType");
            if (!bestKnownImplementationType.IsInterface)
                _knownImplementationType = bestKnownImplementationType;
        }

        /// <summary>
        /// The services (named and typed) exposed by the component.
        /// </summary>
        /// <value></value>
        public IEnumerable<Service> Services
        {
            get;
            private set;
        }

        /// <summary>
        /// A unique identifier for this component (shared in all sub-contexts.)
        /// This value also appears in Services.
        /// </summary>
        /// <value></value>
        public Service Id
        {
            get;
            private set;
        }

        /// <summary>
        /// Additional data associated with the component.
        /// </summary>
        /// <value></value>
        /// <remarks>Note, component registrations are currently copied into
        /// subcontainers: these properties are shared between all instances of the
        /// registration in all subcontainers.</remarks>
        public IDictionary<string, object> ExtendedProperties
        {
            get;
            private set;
        }

        /// <summary>
        /// For registrations that can determine a single implementation
        /// type (or most generic implementation type in a hierarchy) this
        /// method will return true and provide the type through the
        /// <paramref name="implementationType"/> parameter. For registrations
        /// where the implementation type cannot be determined in advance, it
        /// is recommended that the returned instances are inspected as they
        /// are activated.
        /// </summary>
        /// <param name="implementationType">The implementation type.</param>
        /// <returns>True if an implementation type is known.</returns>
        public bool KnownImplementationType(out Type implementationType)
        {
            implementationType = _knownImplementationType;
            return _knownImplementationType != null;
        }
    }
}
