using System;
using System.Web.Mvc;
using Autofac.Integration.Web.Mvc;
using NUnit.Framework;
using System.Web.Routing;

namespace Autofac.Tests.Integration.Web.Mvc.Area1
{
    class HomeController : IController
    {
        public void Execute(RequestContext controllerContext)
        {
            throw new NotImplementedException();
        }
    }
}

namespace Autofac.Tests.Integration.Web.Mvc.Area2
{
    class HomeController : IController
    {
        public void Execute(RequestContext controllerContext)
        {
            throw new NotImplementedException();
        }
    }
}

namespace Autofac.Tests.Integration.Web.Mvc
{
    [TestFixture]
    public class DefaultControllerIdentificationStrategyFixture
    {

        class HomeController : IController
        {
            public void Execute(RequestContext controllerContext)
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void ControllerTypeServicesInDifferentAreasDoNotMatch()
        {
            var target = new DefaultControllerIdentificationStrategy();
            var route1 = target.ServiceForControllerType(typeof(Area1.HomeController));
            var route2 = target.ServiceForControllerType(typeof(Area2.HomeController));
            Assert.AreNotEqual(route1, route2);
        }

    }
}
