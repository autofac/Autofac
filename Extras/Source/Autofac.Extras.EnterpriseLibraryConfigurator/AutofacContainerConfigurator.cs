using System;
using Autofac;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel;

namespace Autofac.Extras.EnterpriseLibraryConfigurator
{
    /// <summary>
    /// <see cref="Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.IContainerConfigurator"/>
    /// implementation for registering Enterprise Library dependencies into
    /// an Autofac container.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Enterprise Library uses
    /// <see cref="Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.IContainerConfigurator"/>
    /// implementations as a bridge between configuration sources
    /// (<see cref="Microsoft.Practices.EnterpriseLibrary.Common.Configuration.IConfigurationSource"/>),
    /// type registration providers
    /// (<see cref="Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.ITypeRegistrationsProvider"/>),
    /// and inversion of control containers. This implementation allows you to
    /// use Autofac as the container from which Enterprise Library services
    /// get resolved.
    /// </para>
    /// <para>
    /// Setup of a container via a configurator is done using the
    /// <see cref="Microsoft.Practices.EnterpriseLibrary.Common.Configuration.EnterpriseLibraryContainer.ConfigureContainer(IContainerConfigurator,IConfigurationSource)"/>
    /// method. Using this configurator, registering Enterprise Library with
    /// Autofac looks like this:
    /// </para>
    /// <code lang="C#">
    /// // Create the ContainerBuilder and register EntLib.
    /// var builder = new ContainerBuilder();
    /// using (var configSource = ConfigurationSourceFactory.Create())
    /// {
    ///   var configurator = new AutofacContainerConfigurator(builder);
    ///   EnterpriseLibraryContainer.ConfigureContainer(configurator, configSource);
    /// }
    /// 
    /// // Register other services/dependencies, then set the
    /// // service locator.
    /// var container = builder.Build();
    /// var autofacLocator = new AutofacServiceLocator(container);
    /// EnterpriseLibraryContainer.Current = autofacLocator;
    /// </code>
    /// <para>
    /// Note the use of <see cref="T:Autofac.Extras.CommonServiceLocator.AutofacServiceLocator"/>
    /// to set the Enterprise Library service locator. You need to do that so
    /// Enterprise Library can do any internal resolutions it requires.
    /// </para>
    /// <para>
    /// Even though you can use this configurator directly, the simplest way
    /// to get Enterprise Library configured with Autofac is to use the
    /// <see cref="M:Autofac.Extras.EnterpriseLibraryConfigurator.EnterpriseLibraryRegistrationExtensions.RegisterEnterpriseLibrary"/>
    /// extensions. The above sample code becomes much simpler and more
    /// Autofac-styled with the extensions.
    /// </para>
    /// <code lang="C#">
    /// // Create the ContainerBuilder and register EntLib.
    /// var builder = new ContainerBuilder();
    /// builder.RegisterEnterpriseLibrary();
    /// 
    /// // Register other services/dependencies, then set the
    /// // service locator.
    /// var container = builder.Build();
    /// var autofacLocator = new AutofacServiceLocator(container);
    /// EnterpriseLibraryContainer.Current = autofacLocator;
    /// </code>
    /// <para>
    /// Note that using Autofac as the backing store for service resolution
    /// in Enterprise Library, changes in the dependency configuration source
    /// are not directly supported. That is, if the set of configured services
    /// changes, the container is not automatically rebuilt. This is a different
    /// behavior from Unity, which supports configuration change and container
    /// update during application execution.
    /// </para>
    /// <para>
    /// The reason this is important is that some internal Enterprise Library
    /// components make the assumption that the container has a configuration
    /// change manager (<see cref="Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.ConfigurationChangeEventSource"/>)
    /// registered regardless of support. This configurator will add a placeholder
    /// implementation to the container to enable these components to function,
    /// but developers should be aware that no action will take place if configuration
    /// changes even though the component appears to be present in the container.
    /// </para>
    /// </remarks>
    /// <seealso cref="M:Autofac.Extras.EnterpriseLibraryConfigurator.EnterpriseLibraryRegistrationExtensions.RegisterEnterpriseLibrary"/>
    public class AutofacContainerConfigurator : IContainerConfigurator
    {
        /// <summary>
        /// The <see cref="Autofac.ContainerBuilder"/> to which Enterprise Library
        /// registrations should be added.
        /// </summary>
        private ContainerBuilder _builder = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacContainerConfigurator"/> class.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="Autofac.ContainerBuilder"/> to which Enterprise Library
        /// registrations should be added.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="builder" /> is <see langword="null" />.
        /// </exception>
        public AutofacContainerConfigurator(ContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            this._builder = builder;
        }

        /// <summary>
        /// Consume the set of
        /// <see cref="Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.TypeRegistration"/>
        /// objects and configure the associated container.
        /// </summary>
        /// <param name="configurationSource">
        /// Configuration source to read registrations from.
        /// </param>
        /// <param name="rootProvider">
        /// <see cref="Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.ITypeRegistrationsProvider"/>
        /// that knows how to read the <paramref name="configurationSource"/>
        /// and return all relevant type registrations.
        /// </param>
        /// <remarks>
        /// <para>
        /// This method is used by <see cref="M:Microsoft.Practices.EnterpriseLibrary.Common.Configuration.EnterpriseLibraryContainer.ConfigureContainer"/>
        /// to add registrations from a provided <paramref name="configurationSource" />
        /// to the Autofac container.
        /// </para>
        /// <para>
        /// At the end of the registration process, a placeholder configuration
        /// change manager (<see cref="Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.ConfigurationChangeEventSource"/>)
        /// will be added to the container if one hasn't already been added.
        /// This is required because some internal Enterprise Library
        /// components make the assumption that the container has a configuration
        /// change manager registered regardless of support. Developers should
        /// be aware that no action will take place if configuration
        /// changes even though the component appears to be present in the container.
        /// </para>
        /// </remarks>
        public void RegisterAll(IConfigurationSource configurationSource, ITypeRegistrationsProvider rootProvider)
        {
            if (configurationSource == null)
            {
                throw new ArgumentNullException("configurationSource");
            }
            if (rootProvider == null)
            {
                throw new ArgumentNullException("rootProvider");
            }
            foreach (TypeRegistration registration in rootProvider.GetRegistrations(configurationSource))
            {
                this._builder.RegisterTypeRegistration(registration);
            }

            // For some reason you have to manually register an event source
            // with the container. Unity must somehow do this automatically in
            // the UnityContainerConfigurator but I can't find it. You need to
            // do this if you use the Logging application block since the
            // LoggingUpdateCoordinator requires this type. I'm not sure which
            // other components might require it.
            //
            // Autofac doesn't support changing the container after it's already
            // built, so rather than implement a ChangeTrackingContainerConfigurator,
            // we'll just stick a throwaway event source in here.
            this._builder.RegisterInstance(new ConfigurationChangeEventSourceImpl()).As<ConfigurationChangeEventSource>().PreserveExistingDefaults();
        }
    }
}
