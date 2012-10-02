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

namespace Autofac.Configuration.Elements
{

    /// <summary>
    /// Element describing a module registration.
    /// </summary>
    public class ModuleElement : ConfigurationElement
    {
        const string TypeAttributeName = "type";
        const string ParametersElementName = "parameters";
        const string PropertiesElementName = "properties";

        /// <summary>
        /// Gets the type of the module. Must expose <see cref="IModule"/>.
        /// </summary>
        /// <value>The type.</value>
        [ConfigurationProperty(TypeAttributeName, IsRequired = true)]
        public string Type
        {
            get
            {
                return (string)this[TypeAttributeName];
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
    }

}
