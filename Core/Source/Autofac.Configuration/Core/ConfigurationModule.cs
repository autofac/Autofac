using System;
using System.Collections.Generic;
using System.Linq;

namespace Autofac.Configuration.Core
{
    /// <summary>
    /// Base class for a configuration parsing/execution module.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Derived module classes are responsible for reading in configuration settings and populating
    /// the <see cref="Autofac.Configuration.ConfigurationModule.SectionHandler"/> property. The
    /// value there will be used in <see cref="Autofac.Configuration.ConfigurationModule.Load"/> to
    /// convert the configuration into container registrations.
    /// </para>
    /// </remarks>
    /// <seealso cref="Autofac.Configuration.ConfigurationSettingsReader"/>
    /// <seealso cref="Autofac.Configuration.XmlFileReader"/>
    public abstract class ConfigurationModule : Module
    {
        /// <summary>
        /// Gets or sets the configuration registrar.
        /// </summary>
        /// <value>
        /// An <see cref="Autofac.Configuration.IConfigurationRegistrar"/> that will be used as the
        /// strategy for converting the <see cref="Autofac.Configuration.ConfigurationModule.SectionHandler"/>
        /// into component registrations. If this value is <see langword="null" />, the registrar
        /// will be a <see cref="Autofac.Configuration.ConfigurationRegistrar"/>.
        /// </value>
        public IConfigurationRegistrar ConfigurationRegistrar { get; set; }

        /// <summary>
        /// Gets the section handler.
        /// </summary>
        /// <value>
        /// The <see cref="Autofac.Configuration.SectionHandler"/> that will be converted into
        /// component registrations in a container.
        /// </value>
        public SectionHandler SectionHandler { get; protected set; }

        /// <summary>
        /// Executes the conversion of configuration data into component registrations.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="Autofac.ContainerBuilder"/> into which registrations will be placed.
        /// </param>
        /// <remarks>
        /// <para>
        /// This override uses the <see cref="Autofac.Configuration.ConfigurationModule.ConfigurationRegistrar"/>
        /// to convert the <see cref="Autofac.Configuration.ConfigurationModule.SectionHandler"/>
        /// into component registrations in the provided <paramref name="builder" />.
        /// </para>
        /// <para>
        /// If no specific <see cref="Autofac.Configuration.ConfigurationModule.ConfigurationRegistrar"/>
        /// is set, the default <see cref="Autofac.Configuration.ConfigurationRegistrar"/> type will be used.
        /// </para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="builder" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if the <see cref="Autofac.Configuration.ConfigurationModule.SectionHandler"/> is <see langword="null" />.
        /// </exception>
        protected override void Load(ContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            if (this.SectionHandler == null)
            {
                throw new InvalidOperationException(ConfigurationSettingsReaderResources.InitializeSectionHandler);
            }
            var registrar = this.ConfigurationRegistrar ?? new ConfigurationRegistrar();
            registrar.RegisterConfigurationSection(builder, this.SectionHandler);
        }
    }
}
