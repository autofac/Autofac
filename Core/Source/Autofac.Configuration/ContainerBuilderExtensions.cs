using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;

namespace Autofac.Configuration
{
    /// <summary>
    /// Extension methods for <see cref="Autofac.ContainerBuilder"/> used in parsing configuration.
    /// </summary>
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// The default section name that will be searched for.
        /// </summary>
        public const string DefaultSectionName = "autofac";
        
        /// <summary>
        /// Registers the contents of a configuration section into a container builder.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="Autofac.ContainerBuilder"/> that should receive the configured registrations.
        /// </param>
        /// <param name="configurationSection">
        /// The <see cref="Autofac.Configuration.SectionHandler"/> containing the configured registrations.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="builder" /> or <paramref name="configurationSection" /> is <see langword="null" />.
        /// </exception>
        public static void RegisterConfigurationSection(this ContainerBuilder builder, SectionHandler configurationSection)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            if (configurationSection == null)
            {
                throw new ArgumentNullException("configurationSection");
            }

            Assembly defaultAssembly = null;
            if (!string.IsNullOrEmpty(configurationSection.DefaultAssembly))
            {
                defaultAssembly = Assembly.Load(configurationSection.DefaultAssembly);
            }

            RegisterConfiguredModules(builder, configurationSection, defaultAssembly);
            RegisterConfiguredComponents(builder, configurationSection, defaultAssembly);
            RegisterReferencedFiles(builder, configurationSection);
        }

        private static void RegisterConfiguredComponents(ContainerBuilder builder, SectionHandler configurationSection, Assembly defaultAssembly)
        {
            foreach (ComponentElement component in configurationSection.Components)
            {
                var registrar = builder.RegisterType(LoadType(component.Type, defaultAssembly));

                IList<Service> services = new List<Service>();
                if (!string.IsNullOrEmpty(component.Service))
                {
                    var serviceType = LoadType(component.Service, defaultAssembly);
                    if (!string.IsNullOrEmpty(component.Name))
                    {
                        services.Add(new KeyedService(component.Name, serviceType));
                    }
                    else
                    {
                        services.Add(new TypedService(serviceType));
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(component.Name))
                    {
                        throw new ConfigurationErrorsException(String.Format(
                            ConfigurationSettingsReaderResources.ServiceTypeMustBeSpecified, component.Name));
                    }
                }

                foreach (ServiceElement service in component.Services)
                {
                    var serviceType = LoadType(service.Type, defaultAssembly);
                    if (!string.IsNullOrEmpty(service.Name))
                    {
                        services.Add(new KeyedService(service.Name, serviceType));
                    }
                    else
                    {
                        services.Add(new TypedService(serviceType));
                    }
                }

                foreach (var service in services)
                {
                    registrar.As(service);
                }
                foreach (var param in component.Parameters.ToParameters())
                {
                    registrar.WithParameter(param);
                }
                foreach (var prop in component.Properties.ToParameters())
                {
                    registrar.WithProperty(prop);
                }
                foreach (var ep in component.Metadata)
                {
                    registrar.WithMetadata(
                        ep.Name, TypeManipulation.ChangeToCompatibleType(ep.Value, Type.GetType(ep.Type)));
                }
                if (!string.IsNullOrEmpty(component.MemberOf))
                {
                    registrar.MemberOf(component.MemberOf);
                }
                SetScope(component, registrar);
                SetOwnership(component, registrar);
                SetInjectProperties(component, registrar);
            }
        }

        private static void RegisterConfiguredModules(ContainerBuilder builder, SectionHandler configurationSection, Assembly defaultAssembly)
        {
            foreach (ModuleElement moduleElement in configurationSection.Modules)
            {
                var moduleType = LoadType(moduleElement.Type, defaultAssembly);
                var moduleActivator = new ReflectionActivator(
                    moduleType,
                    new BindingFlagsConstructorFinder(BindingFlags.Public),
                    new MostParametersConstructorSelector(),
                    moduleElement.Parameters.ToParameters(),
                    moduleElement.Properties.ToParameters());
                var module = (IModule)moduleActivator.ActivateInstance(new ContainerBuilder().Build(), Enumerable.Empty<Parameter>());
                builder.RegisterModule(module);
            }
        }

        private static void RegisterReferencedFiles(ContainerBuilder builder, SectionHandler configurationSection)
        {
            foreach (FileElement file in configurationSection.Files)
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
        private static void SetInjectProperties<TReflectionActivatorData, TSingleRegistrationStyle>(ComponentElement component, IRegistrationBuilder<object, TReflectionActivatorData, TSingleRegistrationStyle> registrar)
            where TReflectionActivatorData: ReflectionActivatorData
            where TSingleRegistrationStyle: SingleRegistrationStyle
        {
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }
            if (registrar == null)
            {
                throw new ArgumentNullException("registrar");
            }
            if (!string.IsNullOrEmpty(component.InjectProperties))
            {
                switch (component.InjectProperties.ToLower())
                {
                    case "no":
                        break;
                    case "yes":
                        registrar.PropertiesAutowired(PropertyWiringFlags.AllowCircularDependencies);
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
        private static void SetOwnership<TReflectionActivatorData, TSingleRegistrationStyle>(ComponentElement component, IRegistrationBuilder<object, TReflectionActivatorData, TSingleRegistrationStyle> registrar)
            where TReflectionActivatorData: ReflectionActivatorData
            where TSingleRegistrationStyle: SingleRegistrationStyle
        {
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }
            if (registrar == null)
            {
                throw new ArgumentNullException("registrar");
            }
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
        private static void SetScope<TReflectionActivatorData, TSingleRegistrationStyle>(ComponentElement component, IRegistrationBuilder<object, TReflectionActivatorData, TSingleRegistrationStyle> registrar)
            where TReflectionActivatorData: ReflectionActivatorData
            where TSingleRegistrationStyle: SingleRegistrationStyle
        {
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }
            if (registrar == null)
            {
                throw new ArgumentNullException("registrar");
            }
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
        private static Type LoadType(string typeName, Assembly defaultAssembly)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }
            if (typeName.Length == 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, ConfigurationSettingsReaderResources.ArgumentMayNotBeEmpty, "type name"), "typeName");
            }
            var type = Type.GetType(typeName);

            if (type == null && defaultAssembly != null)
            {
                type = defaultAssembly.GetType(typeName, false); // Don't throw on error; we'll check it later.
            }
            if (type == null)
            {
                throw new ConfigurationErrorsException(String.Format(CultureInfo.CurrentCulture, ConfigurationSettingsReaderResources.TypeNotFound, typeName));
            }
            return type;
        }
    }
}
