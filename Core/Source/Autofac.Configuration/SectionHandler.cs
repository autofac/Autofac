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

using System;
using System.ComponentModel;
using System.Configuration;
using System.Reflection;
using System.Xml;

namespace Autofac.Configuration
{
    /// <summary>
    /// Section handler for Autofac configuration.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This configuration section is used for XML-based configuration of an Autofac
    /// container. While it is primarily used from inside <c>app.config</c> or <c>web.config</c>
    /// files, you may also use it with other arbitrary XML files via the
    /// <see cref="Autofac.Configuration.SectionHandler.Deserialize"/> helper method.
    /// </para>
    /// </remarks>
    public class SectionHandler : ConfigurationSection
    {
        private const string ModulesPropertyName = "modules";
        private const string ComponentsPropertyName = "components";
        private const string DefaultAssemblyPropertyName = "defaultAssembly";
        private const string FilesPropertyName = "files";

        /// <summary>
        /// Gets the components to be registered.
        /// </summary>
        /// <value>
        /// A <see cref="Autofac.Configuration.ComponentElementCollection"/> with the list
        /// of individual service components that should be registered.
        /// </value>
        [ConfigurationProperty(ComponentsPropertyName, IsRequired = false)]
        public ComponentElementCollection Components
        {
            get
            {
                return (ComponentElementCollection)this[ComponentsPropertyName];
            }
        }

        /// <summary>
        /// Gets the default assembly to search for types in when not explicitly
        /// provided with the type name.
        /// </summary>
        /// <value>
        /// An <see cref="System.Reflection.Assembly"/> that should be used as the default assembly
        /// in type searches.
        /// </value>
        [ConfigurationProperty(DefaultAssemblyPropertyName, IsRequired = false)]
        [TypeConverter(typeof(AssemblyNameConverter))]
        public virtual Assembly DefaultAssembly
        {
            get
            {
                return (Assembly)this[DefaultAssemblyPropertyName];
            }
        }

        /// <summary>
        /// Gets additional configuration files.
        /// </summary>
        /// <value>
        /// A <see cref="Autofac.Configuration.FileElementCollection"/> with the list
        /// of external/referenced configuration files.
        /// </value>
        [ConfigurationProperty(FilesPropertyName, IsRequired = false)]
        public FileElementCollection Files
        {
            get
            {
                return (FileElementCollection)this[FilesPropertyName];
            }
        }

        /// <summary>
        /// Gets the modules to be registered.
        /// </summary>
        /// <value>
        /// A <see cref="Autofac.Configuration.ModuleElementCollection"/> with the list
        /// of modules that should be registered.
        /// </value>
        [ConfigurationProperty(ModulesPropertyName, IsRequired = false)]
        public ModuleElementCollection Modules
        {
            get
            {
                return (ModuleElementCollection)this[ModulesPropertyName];
            }
        }

        /// <summary>
        /// Deserializes a configuration section handler from a specific block of XML.
        /// </summary>
        /// <param name="reader">
        /// The <see cref="System.Xml.XmlReader"/> used to read the XML configuration from the source.
        /// </param>
        /// <returns>
        /// A read/parsed <see cref="SectionHandler"/> based on the contents of the <paramref name="reader"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="reader"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="System.Configuration.ConfigurationErrorsException">
        /// Thrown if <paramref name="reader"/> does not contain XML configuration that can be parsed into
        /// a <see cref="SectionHandler"/>.
        /// </exception>
        public static SectionHandler Deserialize(XmlReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            reader.MoveToContent();
            if (reader.EOF)
            {
                throw new ConfigurationErrorsException("No XML content nodes found in configuration. Check the XML reader to ensure configuration is in place.");
            }
            var handler = new SectionHandler();
            handler.DeserializeElement(reader, false);
            return handler;
        }
    }
}
