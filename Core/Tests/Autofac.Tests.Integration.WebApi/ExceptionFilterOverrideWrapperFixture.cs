using System.Web.Http.Filters;
using Autofac.Integration.WebApi;
using NUnit.Framework;

namespace Autofac.Tests.Integration.WebApi
{
    [TestFixture]
    internal class ExceptionFilterOverrideWrapperFixture
    {
        [Test]
        public void MetadataKeyReturnsOverrideValue()
        {
            var wrapper = new ExceptionFilterOverrideWrapper(new FilterMetadata());
            Assert.That(wrapper.MetadataKey, Is.EqualTo(AutofacWebApiFilterProvider.ExceptionFilterOverrideMetadataKey));
        }

        [Test]
        public void FiltersToOverrideReturnsCorrectType()
        {
            var wrapper = new ExceptionFilterOverrideWrapper(new FilterMetadata());
            Assert.That(wrapper.FiltersToOverride, Is.EqualTo(typeof(IExceptionFilter)));
        }
    }
}