using System.Web.Http.Filters;
using Autofac.Integration.WebApi;
using NUnit.Framework;

namespace Autofac.Tests.Integration.WebApi
{
    [TestFixture]
    internal class AuthorizationFilterOverrideWrapperFixture
    {
        [Test]
        public void MetadataKeyReturnsOverrideValue()
        {
            var wrapper = new AuthorizationFilterOverrideWrapper(new FilterMetadata());
            Assert.That(wrapper.MetadataKey, Is.EqualTo(AutofacWebApiFilterProvider.AuthorizationFilterOverrideMetadataKey));
        }

        [Test]
        public void FiltersToOverrideReturnsCorrectType()
        {
            var wrapper = new AuthorizationFilterOverrideWrapper(new FilterMetadata());
            Assert.That(wrapper.FiltersToOverride, Is.EqualTo(typeof(IAuthorizationFilter)));
        }
    }
}