using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Autofac;
using Autofac.Builder;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel;

namespace Autofac.Extras.EnterpriseLibraryConfigurator
{
    /// <summary>
    /// Extension methods that assist in adding Enterprise Library type
    /// registrations to an Autofac container.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Microsoft Patterns and Practices Enterprise Library comes with an
    /// abstraction around dependency registration. These extension methods
    /// help translate these more general
    /// <see cref="Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.TypeRegistration"/>
    /// instances into Autofac-specific registrations.
    /// </para>
    /// <para>
    /// This is particularly useful in conjunction with the <see cref="T:Autofac.Extras.CommonServiceLocator.AutofacServiceLocator"/>
    /// when you want to use Autofac as the mechanism for resolving Enterprise Library
    /// application block dependencies rather than Unity.
    /// </para>
    /// <para>
    /// The primary Enterprise Library entry point is
    /// <see cref="M:Autofac.Extras.EnterpriseLibraryConfigurator.EnterpriseLibraryRegistrationExtensions.RegisterEnterpriseLibrary"/>.
    /// If you're trying to get Enterprise Library application blocks to use
    /// Autofac, start there.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para>
    /// The simplest way to get started with Enterprise Library and Autofac is
    /// via <see cref="Autofac.Extras.EnterpriseLibraryConfigurator.EnterpriseLibraryRegistrationExtensions.RegisterEnterpriseLibrary(ContainerBuilder)"/>.
    /// While building your Autofac container, use that extension to get the
    /// configuration out of the default Enterprise Library configuration location
    /// and add the appropriate dependency registrations to your container.
    /// </para>
    /// <para>
    /// After the container is built, use the <see cref="T:Autofac.Extras.CommonServiceLocator.AutofacServiceLocator"/>
    /// to set the default Enterprise Library container for any internal resolutions
    /// it needs to make.
    /// </para>
    /// <code lang="C#">
    /// // Create the container and register EntLib configured dependencies.
    /// var builder = new ContainerBuilder();
    /// builder.RegisterEnterpriseLibrary();
    /// 
    /// // Register any other dependencies, then build the container and set
    /// // the default EntLib service locator.
    /// var container = builder.Build();
    /// var autofacLocator = new AutofacServiceLocator(container);
    /// EnterpriseLibraryContainer.Current = autofacLocator;
    /// </code>
    /// </example>
    /// <seealso cref="Autofac.Extras.EnterpriseLibraryConfigurator.AutofacContainerConfigurator"/>
    /// <seealso cref="Autofac.Extras.EnterpriseLibraryConfigurator.AutofacParameterBuilderVisitor"/>
    public static class EnterpriseLibraryRegistrationExtensions
    {
        /// <summary>
        /// Adds dependency registrations from the default Enterprise Library
        /// configuration source to the provided Autofac container builder.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="Autofac.ContainerBuilder"/> to which the registrations
        /// from Enterprise Library should be added.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="builder" /> is <see langword="null" />.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method uses the default Enterprise Library configuration source
        /// from <see cref="Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ConfigurationSourceFactory.Create()"/>
        /// to retrieve the set of configured Enterprise Library components and
        /// dependencies. It takes this set of configuration and adds the appropriate
        /// registrations to the provided <paramref name="builder" />.
        /// </para>
        /// <para>
        /// If you need to provide a specific configuration source from which
        /// the container should be populated, use the
        /// <see cref="Autofac.Extras.EnterpriseLibraryConfigurator.EnterpriseLibraryRegistrationExtensions.RegisterEnterpriseLibrary(ContainerBuilder,IConfigurationSource)"/>
        /// overload of this method.
        /// </para>
        /// </remarks>
        /// <example>
        /// <para>
        /// This example shows a simple setup of registering Enterprise Library
        /// dependencies with Autofac.
        /// </para>
        /// <para>
        /// After the container is built, use the <see cref="T:Autofac.Extras.CommonServiceLocator.AutofacServiceLocator"/>
        /// to set the default Enterprise Library container for any internal resolutions
        /// it needs to make.
        /// </para>
        /// <code lang="C#">
        /// // Create the container and register EntLib configured dependencies.
        /// var builder = new ContainerBuilder();
        /// builder.RegisterEnterpriseLibrary();
        /// 
        /// // Register any other dependencies, then build the container and set
        /// // the default EntLib service locator.
        /// var container = builder.Build();
        /// var autofacLocator = new AutofacServiceLocator(container);
        /// EnterpriseLibraryContainer.Current = autofacLocator;
        /// </code>
        /// </example>
        [ExcludeFromCodeCoverage]
        public static void RegisterEnterpriseLibrary(this ContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            using (var configSource = ConfigurationSourceFactory.Create())
            {
                builder.RegisterEnterpriseLibrary(configSource);
            }
        }

        /// <summary>
        /// Adds dependency registrations from a specified Enterprise Library
        /// configuration source to the provided Autofac container builder.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="Autofac.ContainerBuilder"/> to which the registrations
        /// from Enterprise Library should be added.
        /// </param>
        /// <param name="configSource">
        /// The Enterprise Library configuration source defining the registered
        /// services that should be added to the Autofac container.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="builder" /> or <paramref name="configSource" /> is <see langword="null" />.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method uses the specified Enterprise Library configuration source
        /// from <paramref name="configSource" />
        /// to retrieve the set of configured Enterprise Library components and
        /// dependencies. It takes this set of configuration and adds the appropriate
        /// registrations to the provided <paramref name="builder" />.
        /// </para>
        /// <para>
        /// If you want to use the default Enterprise Library configuration source,
        /// use the
        /// <see cref="Autofac.Extras.EnterpriseLibraryConfigurator.EnterpriseLibraryRegistrationExtensions.RegisterEnterpriseLibrary(ContainerBuilder)"/>
        /// overload of this method.
        /// </para>
        /// </remarks>
        /// <example>
        /// <para>
        /// This example shows a simple setup of registering Enterprise Library
        /// dependencies with Autofac and a specified configuration source.
        /// </para>
        /// <para>
        /// After the container is built, use the <see cref="T:Autofac.Extras.CommonServiceLocator.AutofacServiceLocator"/>
        /// to set the default Enterprise Library container for any internal resolutions
        /// it needs to make.
        /// </para>
        /// <code lang="C#">
        /// // Create the container and register EntLib configured dependencies.
        /// var builder = new ContainerBuilder();
        /// using(var configSource = ConfigurationSourceFactory.Create("config-name"))
        /// {
        ///   builder.RegisterEnterpriseLibrary(configSource);
        /// }
        /// 
        /// // Register any other dependencies, then build the container and set
        /// // the default EntLib service locator.
        /// var container = builder.Build();
        /// var autofacLocator = new AutofacServiceLocator(container);
        /// EnterpriseLibraryContainer.Current = autofacLocator;
        /// </code>
        /// </example>
        [ExcludeFromCodeCoverage]
        public static void RegisterEnterpriseLibrary(this ContainerBuilder builder, IConfigurationSource configSource)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            if (configSource == null)
            {
                throw new ArgumentNullException("configSource");
            }
            var configurator = new AutofacContainerConfigurator(builder);
            EnterpriseLibraryContainer.ConfigureContainer(configurator, configSource);
        }

        /// <summary>
        /// Registers an Enterprise Library type registration with Autofac.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="Autofac.ContainerBuilder"/> to which the registration
        /// should be added.
        /// </param>
        /// <param name="registration">
        /// The <see cref="Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.TypeRegistration"/>
        /// that should be translated and added to the Autofac container.
        /// </param>
        /// <returns>
        /// The registration as converted and added to the <paramref name="builder" />.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="builder" /> or <paramref name="registration" /> is <see langword="null" />.
        /// </exception>
        /// <remarks>
        /// <para>
        /// Generally speaking, developers will not directly interface with this
        /// method. The primary entry point to registering Enterprise Library
        /// dependencies is
        /// <see cref="M:Autofac.Extras.EnterpriseLibraryConfigurator.EnterpriseLibraryRegistrationExtensions.RegisterEnterpriseLibrary"/>.
        /// However, if you have a specific Enterprise Library registration
        /// definition that you need to add to an Autofac container, this method
        /// is how you do it.
        /// </para>
        /// </remarks>
        /// <example>
        /// <para>
        /// In this example, an Enterprise Library type registration is created
        /// manually and added to an Autofac container. In normal circumstances,
        /// you would get the Enterprise Library type registration from a configuration
        /// source.
        /// </para>
        /// <code lang="C#">
        /// // Set up the EntLib type registration.
        /// var registration = new TypeRegistration&lt;MyService&gt;(() =&gt; new MyService());
        /// registration.IsDefault = true;
        /// registration.Lifetime = TypeRegistrationLifetime.Singleton;
        /// 
        /// // Use the extension method to add it to the
        /// // Autofac container.
        /// var builder = new ContainerBuilder();
        /// builder.RegisterTypeRegistration(registration);
        /// var container = builder.Build();
        /// </code>
        /// </example>
        /// <seealso cref="M:Autofac.Extras.EnterpriseLibraryConfigurator.EnterpriseLibraryRegistrationExtensions.RegisterEnterpriseLibrary"/>
        public static IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterTypeRegistration(this ContainerBuilder builder, TypeRegistration registration)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            if (registration == null)
            {
                throw new ArgumentNullException("registration");
            }

            var registrar = builder.RegisterType(registration.ImplementationType);
            if (registration.IsDefault)
            {
                // When IsDefault is set, it implies the registration name should
                // be ignored. Basically, builder.RegisterType<TImplementation>().As<TInterface>();
                registrar.As(registration.ServiceType);
            }
            else if (!String.IsNullOrEmpty(registration.Name))
            {
                // If IsDefault isn't set but there is a name, register the entry
                // as a named service. The name will be "ImplementationType._default_"
                // by default - this is generated inside the EntLib TypeRegistration.DefaultName()
                // method.
                registrar.Named(registration.Name, registration.ServiceType);
            }

            registrar.WithParametersFrom(registration).UsingConstructorFrom(registration).WithInstanceScope(registration.Lifetime);
            return registrar;
        }

        /// <summary>
        /// Sets a constructor preference on an Autofac registration based on
        /// the constructor information in an Enterprise Library type registration.
        /// </summary>
        /// <param name="registrar">
        /// The <see cref="Autofac.Builder.IRegistrationBuilder{TLimit, TActivatorData, TRegistrationStyle}"/>
        /// on which the constructor preference should be set.
        /// </param>
        /// <param name="registration">
        /// The <see cref="Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.TypeRegistration"/>
        /// that has the constructor information to be used on the Autofac registration.
        /// </param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="registrar" /> or <paramref name="registration" /> is <see langword="null" />.
        /// </exception>
        /// <remarks>
        /// <para>
        /// Generally speaking, developers will not directly interface with this
        /// method. The primary entry point to registering Enterprise Library
        /// dependencies is
        /// <see cref="M:Autofac.Extras.EnterpriseLibraryConfigurator.EnterpriseLibraryRegistrationExtensions.RegisterEnterpriseLibrary"/>.
        /// However, if you have a specific Enterprise Library registration
        /// definition that you want to get the constructor information from and
        /// set that on a registration in Autofac, this method is how you do it.
        /// </para>
        /// </remarks>
        /// <example>
        /// <para>
        /// In this example, an Enterprise Library type registration is created
        /// manually and the constructor defined in that registration is set
        /// as the constructor to use when Autofac activates the service. In normal circumstances,
        /// you would get the Enterprise Library type registration from a configuration
        /// source.
        /// </para>
        /// <code lang="C#">
        /// // Set up the EntLib type registration. Note the registration
        /// // uses a constructor with parameters of type string and int.
        /// var registration = new TypeRegistration&lt;MyService&gt;(() =&gt; new MyService("abc", 2));
        /// 
        /// // Use the extension method to set the constructor preference add it to the
        /// // Autofac container.
        /// var builder = new ContainerBuilder();
        /// builder.RegisterType&lt;MyService&gt;().UsingConstructorFrom(registration);
        /// var container = builder.Build();
        /// </code>
        /// </example>
        /// <seealso cref="M:Autofac.Extras.EnterpriseLibraryConfigurator.EnterpriseLibraryRegistrationExtensions.RegisterEnterpriseLibrary"/>
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> UsingConstructorFrom<TLimit, TReflectionActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registrar, TypeRegistration registration) where TReflectionActivatorData : ReflectionActivatorData
        {
            if (registrar == null)
            {
                throw new ArgumentNullException("registrar");
            }
            if (registration == null)
            {
                throw new ArgumentNullException("registration");
            }
            var ctorParameterTypes = registration.NewExpressionBody.Constructor.GetParameters().Select(pi => pi.ParameterType).ToArray();
            registrar.UsingConstructor(ctorParameterTypes);
            return registrar;
        }

        /// <summary>
        /// Sets an object registration to the Autofac equivalent of the provided
        /// Enterprise Library instance scope.
        /// </summary>
        /// <param name="registrar">
        /// The <see cref="Autofac.Builder.IRegistrationBuilder{TLimit, TActivatorData, TRegistrationStyle}"/>
        /// on which the instance scope should be set.
        /// </param>
        /// <param name="lifetime">The Enterprise Library lifetime to translate to Autofac.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        /// <remarks>
        /// <para>
        /// Enterprise Library lifetimes translate to Autofac as follows:
        /// </para>
        /// <list type="table">
        /// <listheader>
        /// <term>Enterprise Library Lifetime</term>
        /// <description>Autofac Equivalent</description>
        /// </listheader>
        /// <item>
        /// <term>
        /// <see cref="Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.TypeRegistrationLifetime.Singleton"/>
        /// </term>
        /// <description>
        /// <see cref="Autofac.Builder.IRegistrationBuilder{TLimit, TActivatorData, TRegistrationStyle}.SingleInstance"/>
        /// </description>
        /// </item>
        /// <item>
        /// <term>
        /// <see cref="Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.TypeRegistrationLifetime.Transient"/>
        /// </term>
        /// <description>
        /// <see cref="Autofac.Builder.IRegistrationBuilder{TLimit, TActivatorData, TRegistrationStyle}.InstancePerDependency"/>
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="registrar" /> is <see langword="null" />.
        /// </exception>
        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> WithInstanceScope<TLimit, TActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registrar, TypeRegistrationLifetime lifetime)
        {
            if (registrar == null)
            {
                throw new ArgumentNullException("registrar");
            }

            // There's only Singleton or Transient.
            if (lifetime == TypeRegistrationLifetime.Transient)
            {
                registrar.InstancePerDependency();
            }
            else
            {
                registrar.SingleInstance();
            }
            return registrar;
        }

        /// <summary>
        /// Inspects the constructor parameters in an Enterprise Library type
        /// registration and applies those to an Autofac service registration.
        /// </summary>
        /// <param name="registrar">
        /// The <see cref="Autofac.Builder.IRegistrationBuilder{TLimit, TActivatorData, TRegistrationStyle}"/>
        /// to which the parameters from the <paramref name="registration" />
        /// should be added.
        /// </param>
        /// <param name="registration">
        /// The <see cref="Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.TypeRegistration"/>
        /// to inspect for constructor parameters and convert to Autofac parameters.
        /// </param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="registrar" /> or <paramref name="registration" /> is <see langword="null" />.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method takes each of the <see cref="Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.ParameterValue"/>
        /// values found in the constructor used by the <paramref name="registration" />
        /// and runs each one through an
        /// <see cref="Autofac.Extras.EnterpriseLibraryConfigurator.AutofacParameterBuilderVisitor"/>.
        /// The converted set of parameters gets added to the <paramref name="registrar" />
        /// for use during object construction.
        /// </para>
        /// <para>
        /// Generally speaking, developers will not directly interface with this
        /// method. The primary entry point to registering Enterprise Library
        /// dependencies is
        /// <see cref="M:Autofac.Extras.EnterpriseLibraryConfigurator.EnterpriseLibraryRegistrationExtensions.RegisterEnterpriseLibrary"/>.
        /// However, if you have a specific Enterprise Library registration
        /// definition that you want to get the constructor parameters from and
        /// set them on a registration in Autofac, this method is how you do it.
        /// </para>
        /// </remarks>
        /// <example>
        /// <para>
        /// In this example, an Enterprise Library type registration is created
        /// manually and the constructor defined in that registration contains
        /// the set of parameters to use when resolving the service from Autofac.
        /// In normal circumstances, you would get the Enterprise Library type
        /// registration from a configuration source.
        /// </para>
        /// <code lang="C#">
        /// // Set up the EntLib type registration. Note the registration
        /// // uses a constructor with parameters of type string and int.
        /// var registration = new TypeRegistration&lt;MyService&gt;(() =&gt; new MyService("abc", 2));
        /// 
        /// // Use the extension method to set the constructor parameters on the
        /// // Autofac container.
        /// var builder = new ContainerBuilder();
        /// builder.RegisterType&lt;MyService&gt;().WithParametersFrom(registration);
        /// var container = builder.Build();
        /// 
        /// // When you resolve the MyService from Autofac, the values "abc" and "2"
        /// // will be passed in - the constructor parameters from the type
        /// // registration.
        /// </code>
        /// </example>
        /// <seealso cref="M:Autofac.Extras.EnterpriseLibraryConfigurator.EnterpriseLibraryRegistrationExtensions.RegisterEnterpriseLibrary"/>
        /// <seealso cref="Autofac.Extras.EnterpriseLibraryConfigurator.AutofacParameterBuilderVisitor"/>
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> WithParametersFrom<TLimit, TReflectionActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registrar, TypeRegistration registration) where TReflectionActivatorData : ReflectionActivatorData
        {
            if (registrar == null)
            {
                throw new ArgumentNullException("registrar");
            }
            if (registration == null)
            {
                throw new ArgumentNullException("registration");
            }

            var reflectedParams = registration.NewExpressionBody.Constructor.GetParameters();
            var ctorParameters = registration.ConstructorParameters.Select(
                (pv, index) =>
                {
                    var parameterInfo = reflectedParams[index];
                    var visitor = new AutofacParameterBuilderVisitor(parameterInfo);
                    visitor.Visit(pv);
                    return visitor.AutofacParameter;
                });

            registrar.WithParameters(ctorParameters);
            return registrar;
        }
    }
}
