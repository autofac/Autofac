using System;
using System.Web.Mvc;
using Autofac.Integration.Web.Mvc;
using NUnit.Framework;
using System.Web.Routing;

namespace Autofac.Tests.Integration.Web.Mvc
{
    [TestFixture]
    public class DefaultControllerIdentificationStrategyFixture
    {
        [Test]
        public void CaseInsensitiveRouteNames()
        {
            var target = new DefaultControllerIdentificationStrategy();
            var route1 = target.ServiceForControllerName("Home");
            var route2 = target.ServiceForControllerName("home");
            Assert.IsNotNull(route1);
            Assert.AreEqual(route1, route2);
        }

        class HomeController : IController
        {
            public void Execute(RequestContext controllerContext)
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void TypeNameMatchesRoute()
        {
            var target = new DefaultControllerIdentificationStrategy();
            var route1 = target.ServiceForControllerType(typeof(HomeController));
            var route2 = target.ServiceForControllerName("home");
            Assert.AreEqual(route1, route2);
        }
    }
}
