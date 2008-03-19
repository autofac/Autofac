// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2008 Autofac Contributors
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
using Autofac.Component.Activation;
using Autofac.Registrars;

namespace Autofac.Configuration
{
    /// <summary>
    /// Configures containers based upon app.config settings.
    /// </summary>
    public class ConfigurationSettingsReader : IModule
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
        /// Configures the container with the registrations from the config file.
        /// </summary>
        /// <param name="container">The container.</param>
        public void Configure(IContainer container)
        {
            Enforce.ArgumentNotNull(container, "container");
            var builder = new ContainerBuilder();
            Configure(builder);
            builder.Build(container);
        }

        /// <summary>
        /// Configures the container builder with the registrations from the config file.
        /// </summary>
        /// <param name="builder">The builder.</param>
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
				var moduleType = LoadType(moduleElement.Type, defaultAssembly);
                var moduleActivator = new ReflectionActivator(
                    moduleType,
                    moduleElement.Parameters.ToDictionary(),
                    moduleElement.ExplicitProperties.ToDictionary());
				var module = (IModule)moduleActivator.ActivateInstance(Context.Empty, ActivationParameters.Empty);
                builder.RegisterModule(module);
			}

            foreach (ComponentElement component in _sectionHandler.Components)
            {
                IList<Service> services = new List<Service>();
                if (!string.IsNullOrEmpty(component.Service))
                    services.Add(new TypedService(LoadType(component.Service, defaultAssembly)));

                foreach (ServiceElement service in component.Services)
                    services.Add(new TypedService(LoadType(service.Type, defaultAssembly)));

                var registrar = builder.Register(LoadType(component.Type, defaultAssembly));

                foreach (var service in services)
                    registrar.As(service);

                registrar.WithArguments(component.Parameters.ToDictionary());

                registrar.WithProperties(component.ExplicitProperties.ToDictionary());

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

        private void SetInjectProperties(ComponentElement component, IReflectiveRegistrar registrar)
        {
            Enforce.ArgumentNotNull(component, "component");
            Enforce.ArgumentNotNull(registrar, "registrar");

            if (!string.IsNullOrEmpty(component.InjectProperties))
            {
                switch (component.InjectProperties.ToLower())
                {
                    case "never":
                        break;
                    case "all":
                        registrar.OnActivated(ActivatedHandler.InjectProperties);
                        break;
                    case "unset":
                        registrar.OnActivated(ActivatedHandler.InjectUnsetProperties);
                        break;
                    default:
                        throw new ConfigurationErrorsException(string.Format(CultureInfo.CurrentCulture,
                            ConfigurationSettingsReaderResources.UnrecognisedInjectProperties, component.InjectProperties));
                }
            }
        }

        private void SetOwnership(ComponentElement component, IReflectiveRegistrar registrar)
        {
            Enforce.ArgumentNotNull(component, "component");
            Enforce.ArgumentNotNull(registrar, "registrar");

            if (!string.IsNullOrEmpty(component.Ownership))
            {
                switch (component.Ownership.ToLower())
                {
                    case "container":
                        registrar.WithOwnership(InstanceOwnership.Container);
                        break;
                    case "external":
                        registrar.WithOwnership(InstanceOwnership.External);
                        break;
                    default:
                        throw new ConfigurationErrorsException(string.Format(CultureInfo.CurrentCulture,
                            ConfigurationSettingsReaderResources.UnrecognisedOwnership, component.Ownership));
                }
            }
        }

        private void SetScope(ComponentElement component, IReflectiveRegistrar registrar)
        {
            Enforce.ArgumentNotNull(component, "component");
            Enforce.ArgumentNotNull(registrar, "registrar");

            if (!string.IsNullOrEmpty(component.Scope))
            {
                switch (component.Scope.ToLower())
                {
                    case "singleton":
                        registrar.WithScope(InstanceScope.Singleton);
                        break;
                    case "container":
                        registrar.WithScope(InstanceScope.Container);
                        break;
                    case "factory":
                        registrar.WithScope(InstanceScope.Factory);
                        break;
                    default:
                        throw new ConfigurationErrorsException(string.Format(CultureInfo.CurrentCulture,
                            ConfigurationSettingsReaderResources.UnrecognisedScope, component.Scope));
                }
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
