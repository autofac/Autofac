// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2009 Autofac Contributors
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
using System.Reflection;
using Autofac.Builder;
using System.Linq;
using Autofac.Core.Activators.Reflection;
using Autofac.Core;
using Autofac.Configuration.Util;

namespace Autofac.Configuration
{

    /// <summary>
    /// Configures containers based upon app.config settings.
    /// </summary>
    public class ConfigurationSettingsReader : Autofac.Module
    {
        /// <summary>
        /// The default section name that will be searched for.
        /// </summary>
        public const string DefaultSectionName = "autofac";

        readonly SectionHandler _sectionHandler;

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
            Enforce.ArgumentNotNull(sectionName, "sectionName");
            Enforce.ArgumentNotNull(configurationFile, "configurationFile");
            
            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = configurationFile;

            var configuration = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            _sectionHandler = (SectionHandler)configuration.GetSection(sectionName);

            if (_sectionHandler == null)
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                          ConfigurationSettingsReaderResources.SectionNotFound, sectionName));
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
        /// Gets the section handler.
        /// </summary>
        /// <value>The section handler.</value>
        protected virtual SectionHandler SectionHandler
        {
            get { return _sectionHandler; }
        }

        /// <summary>
        /// Override to add registrations to the container.
        /// </summary>
        /// <param name="builder">The builder.</param>
        protected override void Load(ContainerBuilder builder)
        {
            Enforce.ArgumentNotNull(builder, "builder");

            Assembly defaultAssembly = null;
            if (!string.IsNullOrEmpty(_sectionHandler.DefaultAssembly))
            {
                defaultAssembly = Assembly.Load(_sectionHandler.DefaultAssembly);
            }

			foreach (ModuleElement moduleElement in _sectionHandler.Modules)
			{
				var moduleType = LoadType(moduleElement.Type, defaultAssembly);
                var moduleActivator = new ReflectionActivator(
                    moduleType,
                    new BindingFlagsConstructorFinder(BindingFlags.Public),
                    new MostParametersConstructorSelector(),
                    moduleElement.Parameters.ToParameters(),
                    moduleElement.Properties.ToParameters());
				var module = (IModule)moduleActivator.ActivateInstance(Container.Empty, Enumerable.Empty<Parameter>());
                builder.RegisterModule(module);
			}

            foreach (ComponentElement component in _sectionHandler.Components)
            {
                IList<Service> services = new List<Service>();
                if (!string.IsNullOrEmpty(component.Service))
                    services.Add(new TypedService(LoadType(component.Service, defaultAssembly)));

                foreach (ServiceElement service in component.Services)
                    services.Add(new TypedService(LoadType(service.Type, defaultAssembly)));

                var registrar = builder.RegisterType(LoadType(component.Type, defaultAssembly));

                foreach (var service in services)
                    registrar.As(service);

                foreach (var param in component.Parameters.ToParameters())
                    registrar.WithParameter(param);

                foreach (var prop in component.Properties.ToParameters())
                    registrar.WithProperty(prop);

                foreach (var ep in component.ExtendedProperties)
                    registrar.WithExtendedProperty(
                        ep.Name, TypeManipulation.ChangeToCompatibleType(ep.Value, Type.GetType(ep.Type)));

                if (!string.IsNullOrEmpty(component.MemberOf))
                    registrar.MemberOf(component.MemberOf);

                if (!string.IsNullOrEmpty(component.Name))
                    registrar.Named(component.Name);

                SetScope(component, registrar);
                SetOwnership(component, registrar);
                SetInjectProperties(component, registrar);
            }

            foreach (FileElement file in _sectionHandler.Files)
            {
                var section = DefaultSectionName;
                if (!string.IsNullOrEmpty(file.Section))
                    section = file.Section;

                var reader = new ConfigurationSettingsReader(section, file.Name);
                builder.RegisterModule(reader);
            }
        }

        /// <summary>
        /// Sets the property injection mode for the component.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="registrar">The registrar.</param>
        protected virtual void SetInjectProperties<TReflectionActivatorData, TSingleRegistrationStyle>(ComponentElement component, RegistrationBuilder<object, TReflectionActivatorData, TSingleRegistrationStyle> registrar)
            where TReflectionActivatorData : ReflectionActivatorData
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            Enforce.ArgumentNotNull(component, "component");
            Enforce.ArgumentNotNull(registrar, "registrar");

            if (!string.IsNullOrEmpty(component.InjectProperties))
            {
                switch (component.InjectProperties.ToLower())
                {
                    case "no":
                        break;
                    case "yes":
                        registrar.PropertiesAutowired(true);
                        break;
                    default:
                        throw new ConfigurationErrorsException(string.Format(CultureInfo.CurrentCulture,
                            ConfigurationSettingsReaderResources.UnrecognisedInjectProperties, component.InjectProperties));
                }
            }
        }

        /// <summary>
        /// Sets the ownership model of the component.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="registrar">The registrar.</param>
        protected virtual void SetOwnership<TReflectionActivatorData, TSingleRegistrationStyle>(ComponentElement component, RegistrationBuilder<object, TReflectionActivatorData, TSingleRegistrationStyle> registrar)
            where TReflectionActivatorData : ReflectionActivatorData
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            Enforce.ArgumentNotNull(component, "component");
            Enforce.ArgumentNotNull(registrar, "registrar");

            if (!string.IsNullOrEmpty(component.Ownership))
            {
                switch (component.Ownership.ToLower())
                {
                    case "lifetime-scope":
                        registrar.OwnedByLifetimeScope();
                        break;
                    case "external":
                        registrar.ExternallyOwned();
                        break;
                    default:
                        throw new ConfigurationErrorsException(string.Format(CultureInfo.CurrentCulture,
                            ConfigurationSettingsReaderResources.UnrecognisedOwnership, component.Ownership));
                }
            }
        }

        /// <summary>
        /// Sets the scope model for the component.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="registrar">The registrar.</param>
        protected virtual void SetScope<TReflectionActivatorData, TSingleRegistrationStyle>(ComponentElement component, RegistrationBuilder<object, TReflectionActivatorData, TSingleRegistrationStyle> registrar)
            where TReflectionActivatorData : ReflectionActivatorData
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            Enforce.ArgumentNotNull(component, "component");
            Enforce.ArgumentNotNull(registrar, "registrar");

            if (!string.IsNullOrEmpty(component.InstanceScope))
            {
                switch (component.InstanceScope.ToLower())
                {
                    case "single-instance":
                        registrar.SingleInstance();
                        break;
                    case "per-lifetime-scope":
                        registrar.InstancePerLifetimeScope();
                        break;
                    case "per-dependency":
                        registrar.InstancePerDependency();
                        break;
                    default:
                        throw new ConfigurationErrorsException(string.Format(CultureInfo.CurrentCulture,
                            ConfigurationSettingsReaderResources.UnrecognisedScope, component.InstanceScope));
                }
            }
        }

        /// <summary>
        /// Loads the type.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="defaultAssembly">The default assembly.</param>
        /// <returns></returns>
        protected virtual Type LoadType(string typeName, Assembly defaultAssembly)
        {
            Enforce.ArgumentNotNull(typeName, "typeName");
            if (typeName == string.Empty)
                throw new ArgumentOutOfRangeException("typeName");

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
