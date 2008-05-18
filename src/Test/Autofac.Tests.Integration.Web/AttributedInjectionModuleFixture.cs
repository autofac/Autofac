using System;
using System.Web;
using Autofac.Builder;
using Autofac.Integration.Web;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Web
{
    [TestFixture]
    public class AttributedInjectionModuleFixture
    {
        const string ContextSuppliedString = "ContextSuppliedString";
        const string ExplicitlyProvidedString = "ExplicitlyProvidedString";

        class HttpHandler : IHttpHandler
        {
            #region IHttpHandler Members

            public bool IsReusable
            {
                get { throw new NotImplementedException(); }
            }

            public void ProcessRequest(HttpContext context)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        [InjectProperties]
        class PropertyInjectedPage : HttpHandler
        {
            public string Property { get; set; }
        }

        [InjectUnsetProperties]
        class UnsetPropertyInjectedPage : HttpHandler
        {
            public string Property { get; set; }
        }

        class NonInjectedPage : HttpHandler
        {
            public string Property { get; set; }
        }

        [Test]
        public void PropertyInjected()
        {
            var context = CreateContext();
            var page = new PropertyInjectedPage();
            var target = new AttributedInjectionModule();
            var injector = target.GetInjectionBehaviour(page);
            Assert.IsNotNull(injector);
            injector.InjectDependencies(context, page);
            Assert.AreEqual(ContextSuppliedString, page.Property);
        }

        [Test]
        public void PropertyInjectedValueSet()
        {
            var context = CreateContext();
            var page = new PropertyInjectedPage();
            page.Property = ExplicitlyProvidedString;
            var target = new AttributedInjectionModule();
            var injector = target.GetInjectionBehaviour(page);
            Assert.IsNotNull(injector);
            injector.InjectDependencies(context, page);
            Assert.AreEqual(ContextSuppliedString, page.Property);
        }

        [Test]
        public void UnsetPropertyInjected()
        {
            var context = CreateContext();
            var page = new UnsetPropertyInjectedPage();
            var target = new AttributedInjectionModule();
            var injector = target.GetInjectionBehaviour(page);
            Assert.IsNotNull(injector);
            injector.InjectDependencies(context, page);
            Assert.AreEqual(ContextSuppliedString, page.Property);
        }

        [Test]
        public void PropertyNotInjectedWhenValueSet()
        {
            var context = CreateContext();
            var page = new UnsetPropertyInjectedPage();
            page.Property = ExplicitlyProvidedString;
            var target = new AttributedInjectionModule();
            var injector = target.GetInjectionBehaviour(page);
            Assert.IsNotNull(injector);
            injector.InjectDependencies(context, page);
            Assert.AreEqual(ExplicitlyProvidedString, page.Property);
        }

        [Test]
        public void PropertyNotInjected()
        {
            var context = CreateContext();
            var page = new NonInjectedPage();
            var target = new AttributedInjectionModule();
            var injector = target.GetInjectionBehaviour(page);
            Assert.IsNotNull(injector);
            injector.InjectDependencies(context, page);
            Assert.IsNull(page.Property);
        }

        private IContext CreateContext()
        {
            var cb = new ContainerBuilder();
            cb.Register(ContextSuppliedString);
            return cb.Build();
        }
    }
}
