using System;
using Autofac.Builder;
using NUnit.Framework;

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

        //[Test]
        //public void ResolveSingletonInContextGivesMeaningfulError()
        //{
        //    var builder = new ContainerBuilder();

        //    builder.RegisterDelegate(c => new HomeController()).Singleton.InContext("request");

        //    var containerApplication = builder.Build();
        //    containerApplication.TagWith("application");

        //    var containerRequest = containerApplication.BeginLifetimeScope();
        //    containerRequest.TagWith("request");

        //    Exception thrown = null;
        //    try
        //    {
        //        var controller = containerRequest.Resolve<HomeController>();
        //    }
        //    catch (Exception ex)
        //    {
        //        thrown = ex;
        //    }

        //    Assert.IsNotNull(thrown);
        //    Assert.IsTrue(thrown.Message.ToLower().Contains("singleton"));
        //}
    }
}
