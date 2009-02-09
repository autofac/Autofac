using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac.Builder;

namespace Autofac.Tests.Component.Activation
{
    [TestFixture]
    public class ServiceListActivatorFixture
    {
        [Test]
        public void ResolveParametersPassedThroughToCollectionElements()
        {
            var builder = new ContainerBuilder();
            builder.RegisterCollection<string>();
            builder.Register((c, p) => p.Named<string>("s"))
                .FactoryScoped()
                .MemberOf<IEnumerable<string>>();
            var container = builder.Build();

            var s = "Howdy, Earth!";
            var strings = container.Resolve<IEnumerable<string>>(new NamedParameter("s", s));
            Assert.IsTrue(strings.Contains(s));
        }
    }
}
