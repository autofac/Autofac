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
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;

namespace Autofac.Configuration
{
    /// <summary>
    /// Configures containers based upon app.config settings.
    /// </summary>
    public class ConfigurationSettingsReader : Module
    {
        /// <summary>
        /// The default section name that will be searched for.
        /// </summary>
        public const string DefaultSectionName = "autofac";

        private readonly SectionHandler _sectionHandler;
        private readonly string _configurationDirectory = Path.GetDirectoryName(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

        /// <summary>
        /// Gets the section handler.
        /// </summary>
        /// <value>The section handler.</value>
        protected virtual SectionHandler SectionHandler
        {
            get
            {
                return _sectionHandler;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationSettingsReader"/> class.
        /// The reader will look for a 'autofac' section.
        /// </summary>
        public ConfigurationSettingsReader()
            : this(DefaultSectionName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationSettingsReader"/> class.
        /// </summary>
        /// <param name="sectionName">Name of the configuration section.</param>
        /// <param name="configurationFile">The configuration file.</param>
        public ConfigurationSettingsReader(string sectionName, string configurationFile)
        {
            if (sectionName == null)
            {
                throw new ArgumentNullException("sectionName");
            }
            if (configurationFile == null)
            {
                throw new ArgumentNullException("configurationFile");
            }
            if (!Path.IsPathRooted(configurationFile))
            {
                configurationFile = Path.Combine(_configurationDirectory, configurationFile);
            }
            var map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = configurationFile;

            var configuration = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            _sectionHandler = (SectionHandler)configuration.GetSection(sectionName);

            if (_sectionHandler == null)
            {
                throw new ConfigurationErrorsException(String.Format(CultureInfo.CurrentCulture, ConfigurationSettingsReaderResources.SectionNotFound, sectionName));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationSettingsReader"/> class.
        /// </summary>
        /// <param name="sectionName">Name of the configuration section.</param>
        public ConfigurationSettingsReader(string sectionName)
        {
            if (sectionName == null)
            {
                throw new ArgumentNullException("sectionName");
            }
            _sectionHandler = (SectionHandler)ConfigurationManager.GetSection(sectionName);

            if (_sectionHandler == null)
            {
                throw new ConfigurationErrorsException(String.Format(CultureInfo.CurrentCulture, ConfigurationSettingsReaderResources.SectionNotFound, sectionName));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationSettingsReader"/> class.
        /// </summary>
        /// <param name="reader">An <see cref="System.Xml.XmlReader"/> that contains the configuration.</param>
        public ConfigurationSettingsReader(XmlReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            _sectionHandler = SectionHandler.Deserialize(reader);
        }

        /// <summary>
        /// Override to add registrations to the container.
        /// </summary>
        /// <param name="builder">The builder.</param>
        protected override void Load(ContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            builder.RegisterConfigurationSection(this._sectionHandler);
        }
    }
}
