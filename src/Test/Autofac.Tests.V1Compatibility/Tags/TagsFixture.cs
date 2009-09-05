using System;
using Autofac.Builder;
using NUnit.Framework;

namespace Autofac.Tests.V1Compatibility.Tags
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

            builder.Register(c => new HomeController()).InContext("request").SingletonScoped();

            var containerApplication = builder.Build();
            containerApplication.TagWith("application");

            var containerRequest = containerApplication.CreateInnerContainer();
            containerRequest.TagWith("request");

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
