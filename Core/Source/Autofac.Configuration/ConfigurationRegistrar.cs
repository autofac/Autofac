using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Autofac.Builder;
using Autofac.Configuration.Elements;
using Autofac.Configuration.Util;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;

namespace Autofac.Configuration
{
    /// <summary>
    /// Default service for adding configured registrations to a container.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This default implementation of <see cref="Autofac.Configuration.IConfigurationRegistrar"/>
    /// processes <see cref="Autofac.Configuration.SectionHandler"/> contents into registrations into
    /// a <see cref="Autofac.ContainerBuilder"/>. You may derive and override to extend the functionality
    /// or you may implement your own <see cref="Autofac.Configuration.IConfigurationRegistrar"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="Autofac.Configuration.IConfigurationRegistrar"/>
    public class ConfigurationRegistrar : IConfigurationRegistrar
    {
        private IEnumerable<Service> EnumerateComponentServices(ComponentElement component, Assembly defaultAssembly)
        {
            if (!string.IsNullOrEmpty(component.Service))
            {
                var serviceType = LoadType(component.Service, defaultAssembly);
                if (!string.IsNullOrEmpty(component.Name))
                {
                    yield return new KeyedService(component.Name, serviceType);
                }
                else
                {
                    yield return new TypedService(serviceType);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(component.Name))
                {
                    throw new ConfigurationErrorsException(String.Format(CultureInfo.CurrentCulture,
                        ConfigurationSettingsReaderResources.ServiceTypeMustBeSpecified, component.Name));
                }
            }

            foreach (ServiceElement service in component.Services)
            {
                var serviceType = LoadType(service.Type, defaultAssembly);
                if (!string.IsNullOrEmpty(service.Name))
                {
                    yield return new KeyedService(service.Name, serviceType);
                }
                else
                {
                    yield return new TypedService(serviceType);
                }
            }
        }

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
        /// Thrown if <paramref name="builder"/> or <paramref name="configurationSection"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method is the primary entry point to configuration section registration. From here,
        /// the various modules, components, and referenced files get registered. You may override
        /// any of those behaviors for a custom registrar if you wish to extend registration behavior.
        /// </para>
        /// </remarks>
        public virtual void RegisterConfigurationSection(ContainerBuilder builder, SectionHandler configurationSection)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            if (configurationSection == null)
            {
                throw new ArgumentNullException("configurationSection");
            }

            this.RegisterConfiguredModules(builder, configurationSection);
            this.RegisterConfiguredComponents(builder, configurationSection);
            this.RegisterReferencedFiles(builder, configurationSection);
        }

        /// <summary>
        /// Registers individual configured components into a container builder.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="Autofac.ContainerBuilder"/> that should receive the configured registrations.
        /// </param>
        /// <param name="configurationSection">
        /// The <see cref="Autofac.Configuration.SectionHandler"/> containing the configured registrations.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="builder"/> or <paramref name="configurationSection"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="System.Configuration.ConfigurationErrorsException">
        /// Thrown if there is any issue in parsing the component configuration into registrations.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This is where the individually configured component registrations get added to the <paramref name="builder" />.
        /// The <see cref="Autofac.Configuration.SectionHandler.Components"/> collection from the <paramref name="configurationSection" />
        /// get processed into individual registrations with associated lifetime scope, name, etc.
        /// </para>
        /// <para>
        /// You may influence the process by overriding this whole method or by overriding these individual
        /// parsing subroutines:
        /// </para>
        /// <list type="bullet">
        /// <item>
        /// <term><see cref="Autofac.Configuration.ConfigurationRegistrar.SetLifetimeScope"/></term>
        /// </item>
        /// <item>
        /// <term><see cref="Autofac.Configuration.ConfigurationRegistrar.SetComponentOwnership"/></term>
        /// </item>
        /// <item>
        /// <term><see cref="Autofac.Configuration.ConfigurationRegistrar.SetInjectProperties"/></term>
        /// </item>
        /// </list>
        /// </remarks>
        protected virtual void RegisterConfiguredComponents(ContainerBuilder builder, SectionHandler configurationSection)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            if (configurationSection == null)
            {
                throw new ArgumentNullException("configurationSection");
            }
            foreach (ComponentElement component in configurationSection.Components)
            {
                var registrar = builder.RegisterType(LoadType(component.Type, configurationSection.DefaultAssembly));

                var services = this.EnumerateComponentServices(component, configurationSection.DefaultAssembly);
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
                    registrar.WithMetadata(ep.Name, TypeManipulation.ChangeToCompatibleType(ep.Value, Type.GetType(ep.Type)));
                }
                if (!string.IsNullOrEmpty(component.MemberOf))
                {
                    registrar.MemberOf(component.MemberOf);
                }
                this.SetLifetimeScope(registrar, component.InstanceScope);
                this.SetComponentOwnership(registrar, component.Ownership);
                this.SetInjectProperties(registrar, component.InjectProperties);
            }
        }

        /// <summary>
        /// Registers individual configured modules into a container builder.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="Autofac.ContainerBuilder"/> that should receive the configured registrations.
        /// </param>
        /// <param name="configurationSection">
        /// The <see cref="Autofac.Configuration.SectionHandler"/> containing the configured registrations.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="builder"/> or <paramref name="configurationSection"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="System.Configuration.ConfigurationErrorsException">
        /// Thrown if there is any issue in parsing the module configuration into registrations.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This is where the individually configured component registrations get added to the <paramref name="builder" />.
        /// The <see cref="Autofac.Configuration.SectionHandler.Modules"/> collection from the <paramref name="configurationSection" />
        /// get processed into individual modules which are instantiated and activated inside the <paramref name="builder" />.
        /// </para>
        /// </remarks>
        protected virtual void RegisterConfiguredModules(ContainerBuilder builder, SectionHandler configurationSection)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            if (configurationSection == null)
            {
                throw new ArgumentNullException("configurationSection");
            }
            foreach (ModuleElement moduleElement in configurationSection.Modules)
            {
                var moduleType = this.LoadType(moduleElement.Type, configurationSection.DefaultAssembly);
                IModule module = null;
                using (var moduleActivator = new ReflectionActivator(
                    moduleType,
                    new DefaultConstructorFinder(),
                    new MostParametersConstructorSelector(),
                    moduleElement.Parameters.ToParameters(),
                    moduleElement.Properties.ToParameters()))
                {
                    module = (IModule)moduleActivator.ActivateInstance(new ContainerBuilder().Build(), Enumerable.Empty<Parameter>());
                }
                builder.RegisterModule(module);
            }
        }

        /// <summary>
        /// Registers referenced configuration files into a container builder.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="Autofac.ContainerBuilder"/> that should receive the configured registrations.
        /// </param>
        /// <param name="configurationSection">
        /// The <see cref="Autofac.Configuration.SectionHandler"/> containing the configured registrations.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="builder"/> or <paramref name="configurationSection"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="System.Configuration.ConfigurationErrorsException">
        /// Thrown if there is any issue in processing the referenced files into registrations.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This is where external files referenced in configuration get recursively loaded and added to the <paramref name="builder" />.
        /// The <see cref="Autofac.Configuration.SectionHandler.Files"/> collection from the <paramref name="configurationSection" />
        /// get processed into individual <see cref="Autofac.Configuration.SectionHandler"/> instances, each of which get
        /// registered with the <paramref name="builder" />.
        /// </para>
        /// </remarks>
        protected virtual void RegisterReferencedFiles(ContainerBuilder builder, SectionHandler configurationSection)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            if (configurationSection == null)
            {
                throw new ArgumentNullException("configurationSection");
            }
            foreach (FileElement file in configurationSection.Files)
            {
                var handler = SectionHandler.Deserialize(file.Name, file.Section);
                this.RegisterConfigurationSection(builder, handler);
            }
        }

        /// <summary>
        /// Sets the property injection mode for the component.
        /// </summary>
        /// <param name="registrar">
        /// The component registration on which property injection mode is being set.
        /// </param>
        /// <param name="injectProperties">
        /// The <see cref="System.String"/> configuration value associated with property
        /// injection for this component registration.
        /// </param>
        /// <remarks>
        /// <para>
        /// By default, this implementation understands <see langword="null" />, empty,
        /// or <see langword="false" /> values (<c>false</c>, <c>0</c>, <c>no</c>)
        /// to mean "no property injection should occur" and <see langword="true" />
        /// values (<c>true</c>, <c>1</c>, <c>yes</c>) to mean "property injection
        /// should occur."
        /// </para>
        /// <para>
        /// You may override this method to extend the available grammar for property injection settings.
        /// </para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="registrar" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="System.Configuration.ConfigurationErrorsException">
        /// Thrown if the value for <paramref name="injectProperties" /> is not part of the
        /// recognized grammar.
        /// </exception>
        protected virtual void SetInjectProperties<TReflectionActivatorData, TSingleRegistrationStyle>(IRegistrationBuilder<object, TReflectionActivatorData, TSingleRegistrationStyle> registrar, string injectProperties)
            where TReflectionActivatorData : ReflectionActivatorData
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            if (registrar == null)
            {
                throw new ArgumentNullException("registrar");
            }
            if (String.IsNullOrWhiteSpace(injectProperties))
            {
                return;
            }
            switch (injectProperties.Trim().ToUpperInvariant())
            {
                case "NO":
                case "N":
                case "FALSE":
                case "0":
                    break;
                case "YES":
                case "Y":
                case "TRUE":
                case "1":
                    registrar.PropertiesAutowired(PropertyWiringFlags.AllowCircularDependencies);
                    break;
                default:
                    throw new ConfigurationErrorsException(string.Format(CultureInfo.CurrentCulture, ConfigurationSettingsReaderResources.UnrecognisedInjectProperties, injectProperties));
            }
        }

        /// <summary>
        /// Sets the ownership model for the component.
        /// </summary>
        /// <param name="registrar">
        /// The component registration on which the ownership model is being set.
        /// </param>
        /// <param name="ownership">
        /// The <see cref="System.String"/> configuration value associated with the
        /// ownership model for this component registration.
        /// </param>
        /// <remarks>
        /// <para>
        /// By default, this implementation understands <see langword="null" /> or empty
        /// values to be "default ownership model"; <c>lifetime-scope</c> or <c>LifetimeScope</c>
        /// is "owned by lifetime scope"; and <c>external</c> or <c>ExternallyOwned</c> is
        /// "externally owned."
        /// </para>
        /// <para>
        /// You may override this method to extend the available grammar for component ownership.
        /// </para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="registrar" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="System.Configuration.ConfigurationErrorsException">
        /// Thrown if the value for <paramref name="ownership" /> is not part of the
        /// recognized grammar.
        /// </exception>
        protected virtual void SetComponentOwnership<TReflectionActivatorData, TSingleRegistrationStyle>(IRegistrationBuilder<object, TReflectionActivatorData, TSingleRegistrationStyle> registrar, string ownership)
            where TReflectionActivatorData : ReflectionActivatorData
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            if (registrar == null)
            {
                throw new ArgumentNullException("registrar");
            }
            if (String.IsNullOrWhiteSpace(ownership))
            {
                return;
            }
            switch (ownership.Trim().ToUpperInvariant())
            {
                case "LIFETIME-SCOPE":
                case "LIFETIMESCOPE":
                    registrar.OwnedByLifetimeScope();
                    break;
                case "EXTERNAL":
                case "EXTERNALLYOWNED":
                    registrar.ExternallyOwned();
                    break;
                default:
                    throw new ConfigurationErrorsException(string.Format(CultureInfo.CurrentCulture, ConfigurationSettingsReaderResources.UnrecognisedOwnership, ownership));
            }
        }

        /// <summary>
        /// Sets the lifetime scope for the component.
        /// </summary>
        /// <param name="registrar">
        /// The component registration on which the lifetime scope is being set.
        /// </param>
        /// <param name="lifetimeScope">
        /// The <see cref="System.String"/> configuration value associated with the
        /// lifetime scope for this component registration.
        /// </param>
        /// <remarks>
        /// <para>
        /// By default, this implementation understands <see langword="null" /> or empty
        /// values to be "default ownership model"; <c>single-instance</c> or <c>SingleInstance</c>
        /// is singleton; <c>instance-per-lifetime-scope</c>, <c>InstancePerLifetimeScope</c>, <c>per-lifetime-scope</c>,
        /// or <c>PerLifetimeScope</c> is one instance per nested lifetime scope; and <c>instance-per-dependency</c>,
        /// <c>InstancePerDependency</c>, <c>per-dependency</c>, or <c>PerDependency</c> is
        /// one instance for each resolution call.
        /// </para>
        /// <para>
        /// You may override this method to extend the available grammar for lifetime scope.
        /// </para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="registrar" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="System.Configuration.ConfigurationErrorsException">
        /// Thrown if the value for <paramref name="lifetimeScope" /> is not part of the
        /// recognized grammar.
        /// </exception>
        protected virtual void SetLifetimeScope<TReflectionActivatorData, TSingleRegistrationStyle>(IRegistrationBuilder<object, TReflectionActivatorData, TSingleRegistrationStyle> registrar, string lifetimeScope)
            where TReflectionActivatorData : ReflectionActivatorData
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            if (registrar == null)
            {
                throw new ArgumentNullException("registrar");
            }
            if (String.IsNullOrWhiteSpace(lifetimeScope))
            {
                return;
            }
            switch (lifetimeScope.Trim().ToUpperInvariant())
            {
                case "SINGLEINSTANCE":
                case "SINGLE-INSTANCE":
                    registrar.SingleInstance();
                    break;
                case "INSTANCE-PER-LIFETIME-SCOPE":
                case "INSTANCEPERLIFETIMESCOPE":
                case "PER-LIFETIME-SCOPE":
                case "PERLIFETIMESCOPE":
                    registrar.InstancePerLifetimeScope();
                    break;
                case "INSTANCE-PER-DEPENDENCY":
                case "INSTANCEPERDEPENDENCY":
                case "PER-DEPENDENCY":
                case "PERDEPENDENCY":
                    registrar.InstancePerDependency();
                    break;
                default:
                    throw new ConfigurationErrorsException(string.Format(CultureInfo.CurrentCulture, ConfigurationSettingsReaderResources.UnrecognisedScope, lifetimeScope));
            }
        }

        /// <summary>
        /// Loads a type by name.
        /// </summary>
        /// <param name="typeName">
        /// Name of the <see cref="System.Type"/> to load. This may be a partial type name or a fully-qualified type name.
        /// </param>
        /// <param name="defaultAssembly">
        /// The default <see cref="System.Reflection.Assembly"/> to use in type resolution if <paramref name="typeName" />
        /// is a partial type name.
        /// </param>
        /// <returns>
        /// The resolved <see cref="System.Type"/> based on the specified name.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="typeName" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="typeName" /> is empty.
        /// </exception>
        /// <exception cref="System.Configuration.ConfigurationErrorsException">
        /// Thrown if the specified <paramref name="typeName" /> can't be resolved as a fully-qualified type name and
        /// isn't a partial type name for a <see cref="System.Type"/> found in the <paramref name="defaultAssembly" />.
        /// </exception>
        protected virtual Type LoadType(string typeName, Assembly defaultAssembly)
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
