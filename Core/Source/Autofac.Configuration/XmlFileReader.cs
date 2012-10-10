using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autofac.Configuration.Core;

namespace Autofac.Configuration
{
    /// <summary>
    /// Configures containers based upon XML configuration settings that are not stored in .NET application configuration.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This module type uses raw XML files to initialize configuration settings. These files are expected to have XML
    /// in them that can be deserialized into a <see cref="Autofac.Configuration.SectionHandler"/>. This XML is the same format
    /// as you would see in a standard <c>app.config</c>/<c>web.config</c> for Autofac, but doesn't require the additional
    /// configuration section definition or other .NET application configuration XML around it.
    /// </para>
    /// <para>
    /// If you are storing your configuration settings in a .NET application configuration file
    /// (<c>app.config</c>/<c>web.config</c>) you can use the <see cref="Autofac.Configuration.ConfigurationSettingsReader"/>
    /// module to read the settings right out of the configuration file without having to manually parse the XML.
    /// </para>
    /// </remarks>
    /// <see cref="Autofac.Configuration.ConfigurationSettingsReader"/>
    /// <see cref="Autofac.Configuration.SectionHandler"/>
    public class XmlFileReader : ConfigurationModule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlFileReader"/> class
        /// using a specified XML configuration file.
        /// </summary>
        /// <param name="fileName">
        /// The name of the configuration file containing XML that can deserialize into a <see cref="Autofac.Configuration.SectionHandler"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="fileName" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="fileName" /> is empty.
        /// </exception>
        /// <remarks>
        /// <para>
        /// Relative paths may be specified in relation to the current application folder (where you would normally
        /// find <c>app.config</c> or <c>web.config</c>).
        /// </para>
        /// </remarks>
        public XmlFileReader(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            if (fileName.Length == 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, ConfigurationSettingsReaderResources.ArgumentMayNotBeEmpty, "fileName"), "fileName");
            }
            this.SectionHandler = SectionHandler.Deserialize(fileName);
        }
    }
}
