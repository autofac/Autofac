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
using System.Linq;
using Autofac.Configuration.Core;

namespace Autofac.Configuration
{
    /// <summary>
    /// Configures containers based upon <c>app.config</c>/<c>web.config</c> settings.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This module type uses standard .NET application configuration format files to initialize configuration
    /// settings. By default the standard <c>app.config</c>/<c>web.config</c> is used with a configuration
    /// section named <c>autofac</c>, but you can use the various constructors to override the file location
    /// or configuration section name.
    /// </para>
    /// <para>
    /// If you are storing your configuration settings in a raw XML file (without the additional
    /// <c>&lt;configuration /&gt;</c> wrapper and section definitions you normally see in .NET application
    /// configuration) you can use the <see cref="Autofac.Configuration.XmlFileReader"/> module to specify
    /// the XML file location directly.
    /// </para>
    /// </remarks>
    /// <see cref="Autofac.Configuration.XmlFileReader"/>
    /// <see cref="Autofac.Configuration.SectionHandler"/>
    public class ConfigurationSettingsReader : ConfigurationModule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationSettingsReader"/> class
        /// using the default application configuration file with a configuration section named <c>autofac</c>.
        /// </summary>
        public ConfigurationSettingsReader()
            : this(SectionHandler.DefaultSectionName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationSettingsReader"/> class
        /// using the default application configuration file and a named section.
        /// </summary>
        /// <param name="sectionName">
        /// The name of the configuration section corresponding to a <see cref="Autofac.Configuration.SectionHandler"/>.
        /// </param>
        public ConfigurationSettingsReader(string sectionName)
        {
            if (sectionName == null)
            {
                throw new ArgumentNullException("sectionName");
            }
            this.SectionHandler = (SectionHandler)ConfigurationManager.GetSection(sectionName);
            if (this.SectionHandler == null)
            {
                throw new ConfigurationErrorsException(String.Format(CultureInfo.CurrentCulture, ConfigurationSettingsReaderResources.SectionNotFound, sectionName));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationSettingsReader"/> class
        /// using a named configuration file and section.
        /// </summary>
        /// <param name="sectionName">
        /// The name of the configuration section corresponding to a <see cref="Autofac.Configuration.SectionHandler"/>.
        /// </param>
        /// <param name="configurationFile">
        /// The <c>app.config</c>/<c>web.config</c> format configuration file containing the
        /// named section.
        /// </param>
        public ConfigurationSettingsReader(string sectionName, string configurationFile)
        {
            this.SectionHandler = SectionHandler.Deserialize(configurationFile, sectionName);
        }
    }
}
