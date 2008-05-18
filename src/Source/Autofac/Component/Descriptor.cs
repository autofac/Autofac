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
using System.Globalization;
using System.Linq;

namespace Autofac.Component
{
    /// <summary>
    /// Describes a component and its services.
    /// </summary>
    public class Descriptor : IComponentDescriptor
    {
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

            BestKnownImplementationType = Enforce.ArgumentNotNull(bestKnownImplementationType, "bestKnownImplementationType");
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
        /// Returns the most specific type that the returned intstances are assignable to.
        /// This may be the actual implementation type, a base class, and abstract class, or an
        /// interface.
        /// </summary>
        /// <remarks>
        /// For registrations where the type is an interface or abstract class
        /// it is recommended that the returned instances are inspected as they are activated.
        /// </remarks>
        /// <returns>The best known implementation type.</returns>
        public Type BestKnownImplementationType
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            var servicesDescription = string.Join(", ", Services.Select(s => s.Description).ToArray());
            return string.Format(CultureInfo.CurrentCulture, DescriptorResources.ToStringFormat,
                BestKnownImplementationType.FullName,
                servicesDescription);
        }
    }
}
