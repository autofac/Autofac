using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Lifetime;
using AutofacContrib.EnterpriseLibraryConfigurator;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel;
using NUnit.Framework;
using EntLibContainer = Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.Container;

namespace AutofacContrib.Tests.EnterpriseLibraryConfigurator
{
	[TestFixture]
	public class EnterpriseLibraryRegistrationExtensionsFixture
	{
		[Test(Description = "Tries to register the EntLib bits from a null configuration source.")]
		public void RegisterEnterpriseLibrary_NullConfigurationSource()
		{
			var builder = new ContainerBuilder();
			IConfigurationSource source = null;
			Assert.Throws<ArgumentNullException>(() => builder.RegisterEnterpriseLibrary(source));
		}

		[Test(Description = "Tries to register the EntLib bits into a null builder.")]
		public void RegisterEnterpriseLibrary_NullContainerBuilder()
		{
			ContainerBuilder builder = null;
			var source = new DictionaryConfigurationSource();
			Assert.Throws<ArgumentNullException>(() => builder.RegisterEnterpriseLibrary());
			Assert.Throws<ArgumentNullException>(() => builder.RegisterEnterpriseLibrary(source));
		}

		[Test(Description = "Registers a default service (not named) with no parameters.")]
		public void RegisterTypeRegistration_Default_NoParameters()
		{
			var registration = new TypeRegistration<RegisteredServiceConsumer>(() => new RegisteredServiceConsumer());
			registration.IsDefault = true;
			registration.Lifetime = TypeRegistrationLifetime.Singleton;

			var builder = new ContainerBuilder();
			builder.RegisterTypeRegistration(registration);
			var container = builder.Build();

			var instance = container.Resolve<RegisteredServiceConsumer>();
			Assert.AreEqual("DEFAULTCTOR", instance.CtorParameter, "The default constructor should have been invoked.");
			var instance2 = container.Resolve<RegisteredServiceConsumer>();
			Assert.AreSame(instance, instance2, "The lifetime was not set on the registration.");
		}

		[Test(Description = "Registers a default service (not named) with an enumerated parameter.")]
		public void RegisterTypeRegistration_Default_WithEnumerationParameter()
		{
			var itemNames = new string[]
			{
				"first",
				"second",
				"third"
			};
			var registration = new TypeRegistration<RegisteredServiceConsumer>(() => new RegisteredServiceConsumer(EntLibContainer.ResolvedEnumerable<ISampleService>(itemNames)));
			registration.IsDefault = true;
			registration.Lifetime = TypeRegistrationLifetime.Transient;

			var builder = new ContainerBuilder();
			builder.RegisterTypeRegistration(registration);
			var first = new SampleServiceImpl();
			builder.RegisterInstance(first).Named<ISampleService>("first");
			var second = new SampleServiceImpl();
			builder.RegisterInstance(second).Named<ISampleService>("second");
			var third = new SampleServiceImpl();
			builder.RegisterInstance(third).Named<ISampleService>("third");
			var container = builder.Build();

			var resolved = container.Resolve<RegisteredServiceConsumer>();
			Assert.IsInstanceOf<IEnumerable<ISampleService>>(resolved.CtorParameter, "The constructor parameter was not the right type.");
			var services = ((IEnumerable<ISampleService>)resolved.CtorParameter).ToArray();
			Assert.AreSame(first, services[0], "The first enumerable service was not resolved properly.");
			Assert.AreSame(second, services[1], "The second enumerable service was not resolved properly.");
			Assert.AreSame(third, services[2], "The third enumerable service was not resolved properly.");
		}

		[Test(Description = "Registers a default service (not named) with a simple constructor parameter.")]
		public void RegisterTypeRegistration_Default_WithSimpleParameter()
		{
			var registration = new TypeRegistration<RegisteredServiceConsumer>(() => new RegisteredServiceConsumer("abc"));
			registration.IsDefault = true;
			registration.Lifetime = TypeRegistrationLifetime.Transient;

			var builder = new ContainerBuilder();
			builder.RegisterTypeRegistration(registration);
			var container = builder.Build();

			var instance = container.Resolve<RegisteredServiceConsumer>();
			Assert.AreEqual("abc", instance.CtorParameter, "The string parameter constructor should have been invoked with the appropriate argument.");
			var instance2 = container.Resolve<RegisteredServiceConsumer>();
			Assert.AreNotSame(instance, instance2, "The lifetime was not set on the registration.");
		}

		[Test(Description = "Registers a named service with no parameters.")]
		public void RegisterTypeRegistration_Named_NoParameters()
		{
			var registration = new TypeRegistration<RegisteredServiceConsumer>(() => new RegisteredServiceConsumer());
			registration.Name = "named-service";
			registration.Lifetime = TypeRegistrationLifetime.Singleton;

			var builder = new ContainerBuilder();
			builder.RegisterTypeRegistration(registration);
			var container = builder.Build();

			var instance = container.ResolveNamed<RegisteredServiceConsumer>("named-service"); ;
			Assert.AreEqual("DEFAULTCTOR", instance.CtorParameter, "The default constructor should have been invoked.");
			var instance2 = container.ResolveNamed<RegisteredServiceConsumer>("named-service"); ;
			Assert.AreSame(instance, instance2, "The lifetime was not set on the registration.");
		}

		[Test(Description = "Registers a named service with constructor parameters.")]
		public void RegisterTypeRegistration_Named_WithParameters()
		{
			var registration = new TypeRegistration<RegisteredServiceConsumer>(() => new RegisteredServiceConsumer(EntLibContainer.Resolved<ISampleService>()));
			registration.Name = "named-service";
			registration.Lifetime = TypeRegistrationLifetime.Transient;

			var dependency = new SampleServiceImpl();
			var builder = new ContainerBuilder();
			builder.RegisterTypeRegistration(registration);
			builder.RegisterInstance(dependency).As<ISampleService>();
			var container = builder.Build();

			var instance = container.ResolveNamed<RegisteredServiceConsumer>("named-service"); ;
			Assert.AreSame(dependency, instance.CtorParameter, "The service implementation parameter constructor should have been invoked with the appropriate argument.");
			var instance2 = container.ResolveNamed<RegisteredServiceConsumer>("named-service"); ;
			Assert.AreNotSame(instance, instance2, "The lifetime was not set on the registration.");
		}

		[Test(Description = "Tries to register a type registration into a null ContainerBuilder.")]
		public void RegisterTypeRegistration_NullContainerBuilder()
		{
			var registration = new TypeRegistration<RegisteredServiceConsumer>(() => new RegisteredServiceConsumer("abc"));
			ContainerBuilder builder = null;
			Assert.Throws<ArgumentNullException>(() => builder.RegisterTypeRegistration(registration));
		}

		[Test(Description = "Tries to register a null type registration into a ContainerBuilder.")]
		public void RegisterTypeRegistration_NullTypeRegistration()
		{
			TypeRegistration registration = null;
			var builder = new ContainerBuilder();
			Assert.Throws<ArgumentNullException>(() => builder.RegisterTypeRegistration(registration));
		}

		[Test(Description = "Verifies a successful constructor preference setting on an Autofac registrar.")]
		public void UsingConstructorFrom_MatchingConstructor()
		{
			var registration = new TypeRegistration<RegisteredServiceConsumer>(() => new RegisteredServiceConsumer("abc"));
			var builder = new ContainerBuilder();
			var registrar = builder.RegisterType<RegisteredServiceConsumer>();
			Assert.DoesNotThrow(() => registrar.UsingConstructorFrom(registration));
			builder.RegisterInstance("def").As<String>();
			var container = builder.Build();
			var instance = container.Resolve<RegisteredServiceConsumer>();
			Assert.AreEqual("def", instance.CtorParameter, "The wrong constructor was chosen.");
		}

		[Test(Description = "Setting a constructor preference on an Autofac registrar should fail if the parameters don't match.")]
		public void UsingConstructorFrom_NoMatchingConstructor()
		{
			// There are no two-string constructors for RegisteredServiceConsumer.
			// The use case here is if someone selects UsingConstructorFrom for a
			// TypeRegistration that doesn't match the type being registered by Autofac.
			var registration = new TypeRegistration<ArgumentOutOfRangeException>(() => new ArgumentOutOfRangeException("paramName", "message"));
			var builder = new ContainerBuilder();
			var registrar = builder.RegisterType<RegisteredServiceConsumer>();
			Assert.Throws<ArgumentException>(() => registrar.UsingConstructorFrom(registration));
		}

		[Test(Description = "Tries to set a constructor preference on a null Autofac registrar.")]
		public void UsingConstructorFrom_NullRegistrar()
		{
			var registration = new TypeRegistration<RegisteredServiceConsumer>(() => new RegisteredServiceConsumer("abc"));
			IRegistrationBuilder<RegisteredServiceConsumer, ConcreteReflectionActivatorData, SingleRegistrationStyle> registrar = null;
			Assert.Throws<ArgumentNullException>(() => registrar.UsingConstructorFrom(registration));
		}

		[Test(Description = "Tries to set a constructor preference from a null EntLib type registration.")]
		public void UsingConstructorFrom_NullRegistration()
		{
			TypeRegistration<RegisteredServiceConsumer> registration = null;
			var builder = new ContainerBuilder();
			var registrar = builder.RegisterType<RegisteredServiceConsumer>();
			Assert.Throws<ArgumentNullException>(() => registrar.UsingConstructorFrom(registration));
		}

		[Test(Description = "Checks the translation of EntLib lifetimes to Autofac instance scopes.")]
		[TestCase(TypeRegistrationLifetime.Singleton, InstanceSharing.Shared, typeof(RootScopeLifetime))]
		[TestCase(TypeRegistrationLifetime.Transient, InstanceSharing.None, typeof(CurrentScopeLifetime))]
		public void WithInstanceScope_CheckLifetimeTranslation(TypeRegistrationLifetime entLibLifetime, InstanceSharing autofacInstanceSharing, Type autofacLifetimeType)
		{
			var builder = new ContainerBuilder();
			builder.RegisterType<RegisteredServiceConsumer>().WithInstanceScope(entLibLifetime);
			var container = builder.Build();

			IComponentRegistration autofacRegistration = null;
			Assert.IsTrue(container.ComponentRegistry.TryGetRegistration(new TypedService(typeof(RegisteredServiceConsumer)), out autofacRegistration), "The service type was not registered into the container.");
			Assert.IsInstanceOf(autofacLifetimeType, autofacRegistration.Lifetime, "The registration lifetime type was not correct.");
			Assert.AreEqual(autofacInstanceSharing, autofacRegistration.Sharing, "The registration sharing was not correct.");
		}

		[Test(Description = "Tries to set the instance scope on a null Autofac registrar.")]
		public void WithInstanceScope_NullRegistrar()
		{
			IRegistrationBuilder<RegisteredServiceConsumer, ConcreteReflectionActivatorData, SingleRegistrationStyle> registrar = null;
			Assert.Throws<ArgumentNullException>(() => registrar.WithInstanceScope(TypeRegistrationLifetime.Singleton));
		}

		[Test(Description = "Registers a service given the parameters from a type registration that has no parameters.")]
		public void WithParametersFrom_NoParameters()
		{
			var registration = new TypeRegistration<RegisteredServiceConsumer>(() => new RegisteredServiceConsumer());
			var builder = new ContainerBuilder();
			builder
				.RegisterType<RegisteredServiceConsumer>()
				.Named<RegisteredServiceConsumer>("success")
				.UsingConstructor()
				.WithParametersFrom(registration);
			builder
				.RegisterType<RegisteredServiceConsumer>()
				.Named<RegisteredServiceConsumer>("fail")
				.UsingConstructor(typeof(string))
				.WithParametersFrom(registration);
			var container = builder.Build();

			Assert.DoesNotThrow(() => container.ResolveNamed<RegisteredServiceConsumer>("success"), "No parameters should allow the no-param constructor to run.");
			Assert.Throws<DependencyResolutionException>(() => container.ResolveNamed<RegisteredServiceConsumer>("fail"), "The resolution should fail on the parameterized constructor if no parameters are provided.");
		}

		[Test(Description = "Tries to set constructor parameters on a null Autofac registrar.")]
		public void WithParametersFrom_NullRegistrar()
		{
			var registration = new TypeRegistration<RegisteredServiceConsumer>(() => new RegisteredServiceConsumer("abc"));
			IRegistrationBuilder<RegisteredServiceConsumer, ConcreteReflectionActivatorData, SingleRegistrationStyle> registrar = null;
			Assert.Throws<ArgumentNullException>(() => registrar.WithParametersFrom(registration));
		}

		[Test(Description = "Tries to set a constructor parameters from a null EntLib type registration.")]
		public void WithParametersFrom_NullRegistration()
		{
			TypeRegistration<RegisteredServiceConsumer> registration = null;
			var builder = new ContainerBuilder();
			var registrar = builder.RegisterType<RegisteredServiceConsumer>();
			Assert.Throws<ArgumentNullException>(() => registrar.WithParametersFrom(registration));
		}

		[Test(Description = "Registers a service given the parameters from a type registration that has a simple parameter.")]
		public void WithParametersFrom_SimpleParameter()
		{
			var registration = new TypeRegistration<RegisteredServiceConsumer>(() => new RegisteredServiceConsumer("abc"));
			var builder = new ContainerBuilder();
			builder
				.RegisterType<RegisteredServiceConsumer>()
				.Named<RegisteredServiceConsumer>("fail")
				.UsingConstructor(typeof(ISampleService))
				.WithParametersFrom(registration);
			builder
				.RegisterType<RegisteredServiceConsumer>()
				.Named<RegisteredServiceConsumer>("success")
				.UsingConstructor(typeof(string))
				.WithParametersFrom(registration);
			var container = builder.Build();

			var resolved = container.ResolveNamed<RegisteredServiceConsumer>("success");
			Assert.AreEqual("abc", resolved.CtorParameter, "The parameterized constructor should have received the parameters from the type registration..");
			Assert.Throws<DependencyResolutionException>(() => container.ResolveNamed<RegisteredServiceConsumer>("fail"), "The resolution should fail on the parameterized constructor if the wrong parameters are provided.");
		}

		private interface ISampleService
		{
		}

		private class SampleServiceImpl : ISampleService
		{
		}

		private class RegisteredServiceConsumer
		{
			public object CtorParameter { get; private set; }
			public RegisteredServiceConsumer()
			{
				this.CtorParameter = "DEFAULTCTOR";
			}
			public RegisteredServiceConsumer(ISampleService service)
			{
				this.CtorParameter = service;
			}
			public RegisteredServiceConsumer(IEnumerable<ISampleService> services)
			{
				this.CtorParameter = services;
			}
			public RegisteredServiceConsumer(string input)
			{
				this.CtorParameter = input;
			}
		}
	}
}
