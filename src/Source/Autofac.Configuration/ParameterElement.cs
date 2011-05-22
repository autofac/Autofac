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

namespace Autofac.Configuration
{

    /// <summary>
    /// Element describing a component constructor parameter.
    /// </summary>
    public class ParameterElement : ConfigurationElement
    {
        const string NameAttributeName = "name";
        const string ValueAttributeName = "value";
        internal const string Key = NameAttributeName;

        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        /// <value>The name.</value>
        [ConfigurationProperty(NameAttributeName, IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)this[NameAttributeName];
            }
        }

        /// <summary>
        /// Gets the value used to set the parameter (type will be converted.)
        /// </summary>
        /// <value>The value.</value>
        [ConfigurationProperty(ValueAttributeName, IsRequired = true)]
        public string Value
        {
            get
            {
                return (string)this[ValueAttributeName];
            }
        }
    }

}
