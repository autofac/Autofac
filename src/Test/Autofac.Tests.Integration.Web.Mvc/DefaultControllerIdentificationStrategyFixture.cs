using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac.Integration.Web.Mvc;
using System.Web.Mvc;

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
            public void Execute(ControllerContext controllerContext)
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
