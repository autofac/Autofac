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
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace Autofac.Configuration
{
    /// <summary>
    /// Base for multi-valued configuration elements.
    /// </summary>
    /// <typeparam name="TElementType"></typeparam>
    public class NamedConfigurationElementCollection<TElementType> : ConfigurationElementCollection
        where TElementType : ConfigurationElement
    {
        string _elementName;
        string _elementKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedConfigurationElementCollection&lt;TElementType&gt;"/> class.
        /// </summary>
        /// <param name="elementName">Name of the element.</param>
        /// <param name="elementKey">The element key.</param>
        protected NamedConfigurationElementCollection(string elementName, string elementKey)
        {
            Enforce.ArgumentNotNull(elementName, "elementName");

            if (elementName == "")
                throw new ArgumentOutOfRangeException(elementName);

            Enforce.ArgumentNotNull(elementKey, "elementKey");

            if (elementKey == "")
                throw new ArgumentOutOfRangeException(elementKey);

            _elementName = elementName;
            _elementKey = elementKey;
        }

        /// <summary>
        /// Gets the name used to identify this collection of elements in the configuration file when overridden in a derived class.
        /// </summary>
        /// <value></value>
        /// <returns>The name of the collection; otherwise, an empty string. The default is an empty string.</returns>
        protected override string ElementName
        {
            get
            {
                return _elementName;
            }
        }

        /// <summary>
        /// Gets the type of the <see cref="T:System.Configuration.ConfigurationElementCollection"/>.
        /// </summary>
        /// <value></value>
        /// <returns>The <see cref="T:System.Configuration.ConfigurationElementCollectionType"/> of this collection.</returns>
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        /// <summary>
        /// Indicates whether the specified <see cref="T:System.Configuration.ConfigurationElement"/> exists in the <see cref="T:System.Configuration.ConfigurationElementCollection"/>.
        /// </summary>
        /// <param name="elementName">The name of the element to verify.</param>
        /// <returns>
        /// true if the element exists in the collection; otherwise, false. The default is false.
        /// </returns>
        protected override bool IsElementName(string elementName)
        {
            return elementName != null && elementName == _elementName;
        }

        /// <summary>
        /// Gets or sets the <see cref="TElementType"/> at the specified index.
        /// </summary>
        /// <value></value>
        public TElementType this[int index]
        {
            get
            {
                return base.BaseGet(index) as TElementType;
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }

        /// <summary>
        /// Creates a new <see cref="T:System.Configuration.ConfigurationElement"/>.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Configuration.ConfigurationElement"/>.
        /// </returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return Activator.CreateInstance<TElementType>();
        }

        /// <summary>
        /// Gets the element key for a specified configuration element when overridden in a derived class.
        /// </summary>
        /// <param name="element">The <see cref="T:System.Configuration.ConfigurationElement"/> to return the key for.</param>
        /// <returns>
        /// An <see cref="T:System.Object"/> that acts as the key for the specified <see cref="T:System.Configuration.ConfigurationElement"/>.
        /// </returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            Enforce.ArgumentNotNull(element, "element");

            return (string)element.ElementInformation.Properties[_elementKey].Value;
        }
    }
}
