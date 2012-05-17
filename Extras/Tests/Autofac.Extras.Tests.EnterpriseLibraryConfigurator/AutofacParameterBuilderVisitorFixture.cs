using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Extras.EnterpriseLibraryConfigurator;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel;
using NUnit.Framework;
using EntLibContainer = Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.Container;

namespace Autofac.Extras.Tests.EnterpriseLibraryConfigurator
{
	[TestFixture]
	public class AutofacParameterBuilderVisitorFixture
	{
		[Test(Description = "The generated parameter value should be null prior to calling Visit.")]
		public void AutofacParameter_NullBeforeVisit()
		{
			var p = GetCtorParam<RegisteredServiceConsumer>(typeof(ISampleService));
			var visitor = new AutofacParameterBuilderVisitor(p);
			Assert.IsNull(visitor.AutofacParameter, "The generated parameter should be null prior to visit.");
		}

		[Test(Description = "Attempts to construct a visitor without providing the reflected info on the target parameter.")]
		public void Ctor_NullParameterInfo()
		{
			Assert.Throws<ArgumentNullException>(() => new AutofacParameterBuilderVisitor(null));
		}

		[Test(Description = "Visits a parameter definition that has a constant parameter value.")]
		public void Visit_ConstantValue()
		{
			// Set up EntLib.
			var registration = new TypeRegistration<RegisteredServiceConsumer>(() => new RegisteredServiceConsumer("abc"));
			var registrationParam = registration.ConstructorParameters.First();
			Assert.IsInstanceOf<ConstantParameterValue>(registrationParam, "The parameter should have been seen by EntLib as a constant value.");

			// Visit the parameter to get the Autofac equivalent.
			var ctorParam = this.GetCtorParam<RegisteredServiceConsumer>(typeof(string));
			var visitor = new AutofacParameterBuilderVisitor(ctorParam);
			visitor.Visit(registrationParam);
			var result = visitor.AutofacParameter;
			Assert.IsNotNull(result, "After visiting the registration value, the generated parameter should be set.");

			// Verify the converted parameter resolves correctly.
			var builder = new ContainerBuilder();
			builder.RegisterType<RegisteredServiceConsumer>().UsingConstructor(typeof(string)).WithParameter(result);
			var container = builder.Build();
			var resolved = container.Resolve<RegisteredServiceConsumer>();
			Assert.AreEqual("abc", resolved.CtorParameter, "The constructor parameter was not properly set.");
		}


		[Test(Description = "Visits a parameter definition that has a an enumerable resolved parameter.")]
		public void Visit_ResolvedEnumerableValue()
		{
			// Set up EntLib.
			var itemNames = new string[]
			{
				"first",
				"second",
				"third"
			};
			var registration = new TypeRegistration<RegisteredServiceConsumer>(() => new RegisteredServiceConsumer(EntLibContainer.ResolvedEnumerable<ISampleService>(itemNames)));
			var registrationParam = registration.ConstructorParameters.First();
			Assert.IsInstanceOf<ContainerResolvedEnumerableParameter>(registrationParam, "The parameter should have been seen by EntLib as a resolved enumerable parameter.");

			// Visit the parameter to get the Autofac equivalent.
			var ctorParam = this.GetCtorParam<RegisteredServiceConsumer>(typeof(IEnumerable<ISampleService>));
			var visitor = new AutofacParameterBuilderVisitor(ctorParam);
			visitor.Visit(registrationParam);
			var result = visitor.AutofacParameter;
			Assert.IsNotNull(result, "After visiting the registration value, the generated parameter should be set.");

			// Verify the converted parameter resolves correctly.
			var builder = new ContainerBuilder();
			builder.RegisterType<RegisteredServiceConsumer>().UsingConstructor(typeof(IEnumerable<ISampleService>)).WithParameter(result);
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

		[Test(Description = "Visits a parameter definition that has a single (not enumerable) unnamed resolved parameter.")]
		public void Visit_ResolvedTypedValue()
		{
			// Set up EntLib.
			var registration = new TypeRegistration<RegisteredServiceConsumer>(() => new RegisteredServiceConsumer(EntLibContainer.Resolved<ISampleService>()));
			var registrationParam = registration.ConstructorParameters.First();
			Assert.IsInstanceOf<ContainerResolvedParameter>(registrationParam, "The parameter should have been seen by EntLib as a resolved parameter.");

			// Visit the parameter to get the Autofac equivalent.
			var ctorParam = this.GetCtorParam<RegisteredServiceConsumer>(typeof(ISampleService));
			var visitor = new AutofacParameterBuilderVisitor(ctorParam);
			visitor.Visit(registrationParam);
			var result = visitor.AutofacParameter;
			Assert.IsNotNull(result, "After visiting the registration value, the generated parameter should be set.");

			// Verify the converted parameter resolves correctly.
			var builder = new ContainerBuilder();
			builder.RegisterType<RegisteredServiceConsumer>().UsingConstructor(typeof(ISampleService)).WithParameter(result);
			var service = new SampleServiceImpl();
			builder.RegisterInstance(service).As<ISampleService>();
			var container = builder.Build();
			var resolved = container.Resolve<RegisteredServiceConsumer>();
			Assert.AreSame(service, resolved.CtorParameter, "The constructor parameter was not properly set.");
		}

		[Test(Description = "Visits a parameter definition that has a single (not enumerable) named resolved parameter.")]
		public void Visit_ResolvedTypedNamedValue()
		{
			// Set up EntLib.
			var registration = new TypeRegistration<RegisteredServiceConsumer>(() => new RegisteredServiceConsumer(EntLibContainer.Resolved<ISampleService>("named")));
			var registrationParam = registration.ConstructorParameters.First();
			Assert.IsInstanceOf<ContainerResolvedParameter>(registrationParam, "The parameter should have been seen by EntLib as a resolved parameter.");

			// Visit the parameter to get the Autofac equivalent.
			var ctorParam = this.GetCtorParam<RegisteredServiceConsumer>(typeof(ISampleService));
			var visitor = new AutofacParameterBuilderVisitor(ctorParam);
			visitor.Visit(registrationParam);
			var result = visitor.AutofacParameter;
			Assert.IsNotNull(result, "After visiting the registration value, the generated parameter should be set.");

			// Verify the converted parameter resolves correctly.
			var builder = new ContainerBuilder();
			builder.RegisterType<RegisteredServiceConsumer>().UsingConstructor(typeof(ISampleService)).WithParameter(result);
			var named = new SampleServiceImpl();
			builder.RegisterInstance(named).Named<ISampleService>("named");
			var notNamed = new SampleServiceImpl();
			builder.RegisterInstance(notNamed).As<ISampleService>();
			var container = builder.Build();
			var resolved = container.Resolve<RegisteredServiceConsumer>();
			Assert.AreSame(named, resolved.CtorParameter, "The constructor parameter was not properly set.");
		}

		[Test(Description = "Tries to visit a null constant parameter value.")]
		public void VisitConstantParameterValue_NullValue()
		{
			var ctorParam = this.GetCtorParam<RegisteredServiceConsumer>(typeof(ISampleService));
			var visitor = new PublicVisitor(ctorParam);
			Assert.Throws<ArgumentNullException>(() => visitor.PublicVisitConstantParameterValue(null));
		}

		[Test(Description = "Tries to visit a null enumerable parameter value.")]
		public void VisitEnumerableParameterValue_NullValue()
		{
			var ctorParam = this.GetCtorParam<RegisteredServiceConsumer>(typeof(ISampleService));
			var visitor = new PublicVisitor(ctorParam);
			Assert.Throws<ArgumentNullException>(() => visitor.PublicVisitEnumerableParameterValue(null));
		}

		[Test(Description = "Tries to visit a null resolved parameter value.")]
		public void VisitResolvedParameterValue_NullValue()
		{
			var ctorParam = this.GetCtorParam<RegisteredServiceConsumer>(typeof(ISampleService));
			var visitor = new PublicVisitor(ctorParam);
			Assert.Throws<ArgumentNullException>(() => visitor.PublicVisitResolvedParameterValue(null));
		}

		private ParameterInfo GetCtorParam<T>(params Type[] args)
		{
			return typeof(T).GetConstructor(args).GetParameters()[0];
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

		/// <summary>
		/// Derived class used for testing argument validation.
		/// </summary>
		private class PublicVisitor : AutofacParameterBuilderVisitor
		{
			public PublicVisitor(ParameterInfo methodParameter) : base(methodParameter) { }

			public void PublicVisitConstantParameterValue(ConstantParameterValue parameterValue)
			{
				base.VisitConstantParameterValue(parameterValue);
			}

			public void PublicVisitEnumerableParameterValue(ContainerResolvedEnumerableParameter parameterValue)
			{
				base.VisitEnumerableParameterValue(parameterValue);
			}

			public void PublicVisitResolvedParameterValue(ContainerResolvedParameter parameterValue)
			{
				base.VisitResolvedParameterValue(parameterValue);
			}
		}
	}
}
