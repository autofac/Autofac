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

using System.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace Autofac.Configuration.Elements
{

    /// <summary>
    /// Element describing a component registration.
    /// </summary>
    public class ComponentElement : ConfigurationElement
    {
        const string TypeAttributeName = "type";
        const string ServiceAttributeName = "service";
        const string ServicesElementName = "services";
        const string ParametersElementName = "parameters";
        const string PropertiesElementName = "properties";
        const string MetadataElementName = "metadata";
        const string MemberOfAttributeName = "member-of";
        const string NameAttributeName = "name";
        const string InstanceScopeAttributeName = "instance-scope";
        const string InstanceOwnershipAttributeName = "instance-ownership";
        const string InjectPropertiesAttributeName = "inject-properties";
        internal const string Key = TypeAttributeName;

        /// <summary>
        /// Gets the type of the component.
        /// </summary>
        /// <value>The type.</value>
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        [ConfigurationProperty(TypeAttributeName, IsRequired = true)]
        public string Type
        {
            get
            {
                return (string)this[TypeAttributeName];
            }
        }

        /// <summary>
        /// Gets the service exposed by the component. For multiple-service components,
        /// use the services element instead.
        /// </summary>
        /// <value>The service.</value>
        [ConfigurationProperty(ServiceAttributeName, IsRequired = false)]
        public string Service
        {
            get
            {
                return (string)this[ServiceAttributeName];
            }
        }

        /// <summary>
        /// Allows the component to be added to another composite component.
        /// </summary>
        /// <value>The name of the composite component.</value>
        [ConfigurationProperty(MemberOfAttributeName, IsRequired = false)]
        public string MemberOf
        {
            get
            {
                return (string)this[MemberOfAttributeName];
            }
        }

        /// <summary>
        /// Allows the component to be added to another composite component.
        /// </summary>
        /// <value>The name of the composite component.</value>
        [ConfigurationProperty(NameAttributeName, IsRequired = false)]
        public string Name
        {
            get
            {
                return (string)this[NameAttributeName];
            }
        }

        /// <summary>
        /// Sets the scope of the component instances.
        /// </summary>
        /// <value>singleton (default,) factory or container.</value>
        [ConfigurationProperty(InstanceScopeAttributeName, IsRequired = false)]
        public string InstanceScope
        {
            get
            {
                // TODO: Convert the instance scope specifier to an enumeration.
                return (string)this[InstanceScopeAttributeName];
            }
        }

        /// <summary>
        /// Sets the ownership over the component instances.
        /// </summary>
        /// <value>container (default) or external.</value>
        [ConfigurationProperty(InstanceOwnershipAttributeName, IsRequired = false)]
        public string Ownership
        {
            get
            {
                // TODO: Convert the ownership specifier to an enumeration.
                return (string)this[InstanceOwnershipAttributeName];
            }
        }

        /// <summary>
        /// Sets up property injection for the component instances. This uses the
        /// OnActivated event so that circular dependencies can be handled.
        /// </summary>
        /// <value>never (default,) all, unset.</value>
        [ConfigurationProperty(InjectPropertiesAttributeName, IsRequired = false)]
        public string InjectProperties
        {
            get
            {
                // TODO: Convert property injection to a Boolean.
                return (string)this[InjectPropertiesAttributeName];
            }
        }

        /// <summary>
        /// Gets the services exposed by the component.
        /// </summary>
        /// <value>The services.</value>
        [ConfigurationProperty(ServicesElementName, IsRequired = false)]
        public ServiceElementCollection Services
        {
            get
            {
                return (ServiceElementCollection)this[ServicesElementName];
            }
        }

        /// <summary>
        /// Gets the parameters used to construct the component.
        /// </summary>
        /// <value>The parameters.</value>
        [ConfigurationProperty(ParametersElementName, IsRequired = false)]
        public ParameterElementCollection Parameters
        {
            get
            {
                return (ParameterElementCollection)this[ParametersElementName];
            }
        }

        /// <summary>
        /// Gets the properties to be explicitly set on the component.
        /// </summary>
        /// <value>The explicit properties.</value>
        [ConfigurationProperty(PropertiesElementName, IsRequired = false)]
        public new PropertyElementCollection Properties
        {
            get
            {
                return (PropertyElementCollection)this[PropertiesElementName];
            }
        }

        /// <summary>
        /// Gets the extended properties associated with the registration.
        /// </summary>
        /// <value>The extended properties.</value>
        [ConfigurationProperty(MetadataElementName, IsRequired = false)]
        public MetadataElementCollection Metadata
        {
            get
            {
                return (MetadataElementCollection)this[MetadataElementName];
            }
        }
    }

}
