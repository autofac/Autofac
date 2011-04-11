using System;
using System.Collections.Generic;
using Autofac;
using AutofacContrib.EnterpriseLibraryConfigurator;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel;
using NUnit.Framework;

namespace AutofacContrib.Tests.EnterpriseLibraryConfigurator
{
	[TestFixture]
	public class AutofacContainerConfiguratorFixture
	{
		[Test(Description = "Tries to create a configurator on a null builder.")]
		public void Ctor_NullBuilder()
		{
			Assert.Throws<ArgumentNullException>(() => new AutofacContainerConfigurator(null));
		}

		[Test(Description = "Tries to register dependencies from a null configuration source.")]
		public void RegisterAll_NullConfigurationSource()
		{
			var builder = new ContainerBuilder();
			var configurator = new AutofacContainerConfigurator(builder);
			var rootProvider = new StubRegistrationProvider();
			Assert.Throws<ArgumentNullException>(() => configurator.RegisterAll(null, rootProvider));
		}

		[Test(Description = "Tries to register dependencies using a null registration provider.")]
		public void RegisterAll_NullRegistrationProvider()
		{
			var builder = new ContainerBuilder();
			var configurator = new AutofacContainerConfigurator(builder);
			var configurationSource = new NullConfigurationSource();
			Assert.Throws<ArgumentNullException>(() => configurator.RegisterAll(configurationSource, null));
		}

		[Test(Description = "Verifies that a placeholder configuration change event source is added so EntLib components that require it will find it.")]
		public void RegisterAll_RegistersPlaceholderConfigurationChangeEventSource()
		{
			var container = this.ExecuteRegisterAllOnValidConfigurator();
			Assert.IsTrue(container.IsRegistered<ConfigurationChangeEventSource>(), "A ConfigurationChangeEventSource should have been found in the container.");
		}

		[Test(Description = "Verifies that the provided EntLib registrations get added to the Autofac container.")]
		public void RegisterAll_RegistersProvidedTypeRegistrations()
		{
			var container = this.ExecuteRegisterAllOnValidConfigurator();
			Assert.IsTrue(container.IsRegistered<ISampleService>(), "The provided registration was not added to the container.");
		}

		private IContainer ExecuteRegisterAllOnValidConfigurator()
		{
			var builder = new ContainerBuilder();
			var configurator = new AutofacContainerConfigurator(builder);
			var configurationSource = new NullConfigurationSource();
			var rootProvider = new StubRegistrationProvider();
			configurator.RegisterAll(configurationSource, rootProvider);
			return builder.Build();
		}

		private class StubRegistrationProvider : ITypeRegistrationsProvider
		{
			public IEnumerable<TypeRegistration> GetRegistrations(IConfigurationSource configurationSource)
			{
				yield return new TypeRegistration<ISampleService>(() => new SampleServiceImpl()) { IsDefault = true };
			}

			public IEnumerable<TypeRegistration> GetUpdatedRegistrations(IConfigurationSource configurationSource)
			{
				throw new NotImplementedException();
			}
		}

		private interface ISampleService
		{
		}

		private class SampleServiceImpl : ISampleService
		{
		}
	}
}
