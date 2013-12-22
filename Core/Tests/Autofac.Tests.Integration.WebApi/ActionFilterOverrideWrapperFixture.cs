using System.Web.Http.Filters;
using Autofac.Integration.WebApi;
using NUnit.Framework;

namespace Autofac.Tests.Integration.WebApi
{
    [TestFixture]
    internal class ActionFilterOverrideWrapperFixture
    {
        [Test]
        public void MetadataKeyReturnsOverrideValue()
        {
            var wrapper = new ActionFilterOverrideWrapper(new FilterMetadata());
            Assert.That(wrapper.MetadataKey, Is.EqualTo(AutofacWebApiFilterProvider.ActionFilterOverrideMetadataKey));
        }

        [Test]
        public void FiltersToOverrideReturnsCorrectType()
        {
            var wrapper = new ActionFilterOverrideWrapper(new FilterMetadata());
            Assert.That(wrapper.FiltersToOverride, Is.EqualTo(typeof(IActionFilter)));
        }
    }
}