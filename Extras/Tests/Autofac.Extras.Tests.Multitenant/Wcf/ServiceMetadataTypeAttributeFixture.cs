using System;
using Autofac.Extras.Multitenant.Wcf;
using NUnit.Framework;

namespace Autofac.Extras.Tests.Multitenant.Wcf
{
	[TestFixture]
	public class ServiceMetadataTypeAttributeFixture
	{
		[Test(Description = "Attempts to construct an attribute with a null type value.")]
		public void Ctor_NullType()
		{
			Assert.Throws<ArgumentNullException>(() => new ServiceMetadataTypeAttribute(null));
		}

		[Test(Description = "Verifies the constructor sets appropriate properties.")]
		public void Ctor_SetsProperties()
		{
			var attrib = new ServiceMetadataTypeAttribute(typeof(string));
			Assert.AreEqual(typeof(string), attrib.MetadataClassType, "The metadata class type was not set.");
		}
	}
}
