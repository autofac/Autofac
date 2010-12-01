using System.Web.Mvc;
using Autofac.Integration.Mvc;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Mvc
{
    [TestFixture]
    public class WebViewPageRegistrationSourceFixture
    {
        [Test]
        public void RegistrationFoundForViewWithoutModel()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ViewDependency>().AsSelf();
            builder.RegisterSource(new WebViewPageRegistrationSource());

            var container = builder.Build();
            using (var lifetimeScope = container.BeginLifetimeScope(RequestLifetimeHttpModule.HttpRequestTag))
            {
                var webViewPage = lifetimeScope.Resolve<GeneratedPage>();
                Assert.That(webViewPage, Is.Not.Null);
                Assert.That(webViewPage.Dependency, Is.Not.Null);
            }
        }

        [Test]
        public void RegistrationFoundForViewWithModel()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ViewDependency>().AsSelf();
            builder.RegisterSource(new WebViewPageRegistrationSource());

            var container = builder.Build();
            using (var lifetimeScope = container.BeginLifetimeScope(RequestLifetimeHttpModule.HttpRequestTag))
            {
                var webViewPage = lifetimeScope.Resolve<GeneratedPageWithModel>();
                Assert.That(webViewPage, Is.Not.Null);
                Assert.That(webViewPage.Dependency, Is.Not.Null);
            }
        }
    }

    public class ViewDependency { }

    public class ViewModel { }

    public abstract class AbstractPage : WebViewPage
    {
        public ViewDependency Dependency { get; set; }
    }

    public abstract class AbstractPageWithModel : WebViewPage<Model>
    {
        public ViewDependency Dependency { get; set; }
    }

    public class GeneratedPage : AbstractPage
    {
        public override void Execute() { }
    }

    public class GeneratedPageWithModel : AbstractPageWithModel
    {
        public override void Execute() { }
    }
}