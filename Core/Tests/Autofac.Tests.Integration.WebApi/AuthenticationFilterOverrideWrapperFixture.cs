using System.Web.Http.Filters;
using Autofac.Integration.WebApi;
using NUnit.Framework;

namespace Autofac.Tests.Integration.WebApi
{
    [TestFixture]
    internal class AuthenticationFilterOverrideWrapperFixture
    {
        [Test]
        public void MetadataKeyReturnsOverrideValue()
        {
            var wrapper = new AuthenticationFilterOverrideWrapper(new FilterMetadata());
            Assert.That(wrapper.MetadataKey, Is.EqualTo(AutofacWebApiFilterProvider.AuthenticationFilterOverrideMetadataKey));
        }

        [Test]
        public void FiltersToOverrideReturnsCorrectType()
        {
            var wrapper = new AuthenticationFilterOverrideWrapper(new FilterMetadata());
            Assert.That(wrapper.FiltersToOverride, Is.EqualTo(typeof(IAuthenticationFilter)));
        }
    }
}