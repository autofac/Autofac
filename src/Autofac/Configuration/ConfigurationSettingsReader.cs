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
using System.Reflection;
using System.Globalization;
using Autofac.Component.Registration;
using Autofac.Component.Activation;
using Autofac.Builder;

namespace Autofac.Configuration
{
    /// <summary>
    /// Configures containers based upon app.config settings.
    /// </summary>
    public class ConfigurationSettingsReader
    {
        const string AutofacSectionName = "autofac";

        SectionHandler _sectionHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationSettingsReader"/> class.
        /// The reader will look for a 'autofac' section.
        /// </summary>
        public ConfigurationSettingsReader()
            : this(AutofacSectionName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationSettingsReader"/> class.
        /// </summary>
        /// <param name="sectionName">Name of the configuration section.</param>
        public ConfigurationSettingsReader(string sectionName)
        {
            Enforce.ArgumentNotNull(sectionName, "sectionName");

            _sectionHandler = (SectionHandler)ConfigurationManager.GetSection(sectionName);

            if (_sectionHandler == null)
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                          ConfigurationSettingsReaderResources.SectionNotFound, sectionName));
        }

        /// <summary>
        /// Configures the container with the registrations from the config file.
        /// </summary>
        /// <param name="container">The container.</param>
        public void Configure(Container container)
        {
            Enforce.ArgumentNotNull(container, "container");
            var builder = new ContainerBuilder();
            Configure(builder);
            builder.Build(container);
        }

        /// <summary>
        /// Configures the container builder with the registrations from the config file.
        /// </summary>
        /// <param name="container">The container builder.</param>
        public void Configure(ContainerBuilder builder)
        {
            Enforce.ArgumentNotNull(builder, "builder");

            Assembly defaultAssembly = null;
            if (!string.IsNullOrEmpty(_sectionHandler.DefaultAssembly))
            {
                defaultAssembly = Assembly.Load(_sectionHandler.DefaultAssembly);
            }

			foreach (ModuleElement moduleElement in _sectionHandler.Modules)
			{
				Type moduleType = LoadType(moduleElement.Type, defaultAssembly);
				IModule module = (IModule)Activator.CreateInstance(moduleType);
                builder.RegisterModule(module);
			}

            foreach (ComponentElement component in _sectionHandler.Components)
            {
                IDictionary<string, object> parameters = new Dictionary<string, object>();
                foreach (ParameterElement parameter in component.Parameters)
                    parameters.Add(parameter.Name, parameter.Value);

				IDictionary<string, object> properties = new Dictionary<string, object>();
				foreach (PropertyElement property in component.ExplicitProperties)
					properties.Add(property.Name, property.Value);

                ReflectionActivator activator =
                    new ReflectionActivator(
						LoadType(component.Type, defaultAssembly),
						parameters,
						properties);

                IList<Type> services = new List<Type>();
                if (!string.IsNullOrEmpty(component.Service))
                    services.Add(LoadType(component.Service, defaultAssembly));

                foreach (ServiceElement service in component.Services)
                    services.Add(LoadType(service.Type, defaultAssembly));

                ComponentRegistration cr = new ComponentRegistration(services, activator);
                builder.RegisterComponent(cr);
            }
        }

        Type LoadType(string typeName, Assembly defaultAssembly)
        {
            Enforce.ArgumentNotNull(typeName, "typeName");

            Type type = Type.GetType(typeName);

            if (type == null && defaultAssembly != null)
                type = defaultAssembly.GetType(typeName, false); // Don't throw on error.

            if (type == null)
                throw new ConfigurationErrorsException(string.Format(CultureInfo.CurrentCulture,
                    ConfigurationSettingsReaderResources.TypeNotFound, typeName));

            return type;
        }
    }
}
