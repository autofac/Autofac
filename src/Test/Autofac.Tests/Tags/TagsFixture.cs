using System;
using NUnit.Framework;
using Autofac.Builder;
using Autofac.Tags;

namespace Autofac.Tests.Tags
{
	[TestFixture]
	public class TagsFixture
	{
	    class HomeController
	    {
	        public HomeController()
	        {
	        }
	    }
	
	    [Test]
	    public void ResolveSingletonInContextGivesMeaningfulError()
        {
            var builder = new ContainerBuilder();

            builder.RegisterInContext(c => new HomeController(), "request");

            var containerApplication = builder.Build();
            containerApplication.TagContext("application");

            var containerRequest = containerApplication.CreateInnerContainer();
            containerRequest.TagContext("request");

            Exception thrown = null;
            try
            {
	            var controller = containerRequest.Resolve<HomeController>();
            }
            catch(Exception ex)
            {
            	thrown = ex;
            }
            
            Assert.IsNotNull(thrown);
            Assert.IsTrue(thrown.Message.ToLower().Contains("singleton"));
        }
	}
}
