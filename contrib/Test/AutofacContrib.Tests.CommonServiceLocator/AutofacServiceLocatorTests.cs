using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using AutofacContrib.CommonServiceLocator;
using AutofacContrib.Tests.CommonServiceLocator.Components;
using CommonServiceLocator.AutofacAdapter;

using Microsoft.Practices.ServiceLocation;
using NUnit.Framework;

namespace AutofacContrib.Tests.CommonServiceLocator
{
	[TestFixture]
	public sealed class AutofacServiceLocatorTests : ServiceLocatorTestCase
	{
		protected override IServiceLocator CreateServiceLocator()
		{
			return new AutofacServiceLocator(CreateContainer());
		}

		static IComponentContext CreateContainer()
		{
			var builder = new ContainerBuilder();

		    builder
		        .RegisterType<SimpleLogger>()
		        .Named<ILogger>(typeof (SimpleLogger).FullName)
                .SingleInstance()
		        .As<ILogger>();

			builder
				.RegisterType<AdvancedLogger>()
				.Named<ILogger>(typeof (AdvancedLogger).FullName)
                .SingleInstance()
				.As<ILogger>();

			return builder.Build();
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Constructor_Does_Not_Accept_Null()
		{
			new AutofacServiceLocator(null);
		}
	}
}