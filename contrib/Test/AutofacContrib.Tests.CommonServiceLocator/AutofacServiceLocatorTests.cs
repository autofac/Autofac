using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Builder;
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

		static IContext CreateContainer()
		{
			var builder = new ContainerBuilder();
			builder.RegisterCollection<ILogger>().As<IEnumerable<ILogger>>();

			builder
				.Register<SimpleLogger>()
				.Named(typeof (SimpleLogger).FullName)
				.MemberOf<IEnumerable<ILogger>>();

			builder
				.Register<AdvancedLogger>()
				.Named(typeof (AdvancedLogger).FullName)
				.MemberOf<IEnumerable<ILogger>>()
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