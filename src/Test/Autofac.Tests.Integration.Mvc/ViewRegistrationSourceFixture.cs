using System.Web.Mvc;
using Autofac.Integration.Mvc;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Mvc
{
    [TestFixture]
    public class ViewRegistrationSourceFixture
    {
        [Test]
        public void RegistrationFoundForWebViewPageWithoutModel()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ViewDependency>().AsSelf();
            builder.RegisterSource(new ViewRegistrationSource());

            var container = builder.Build();
            using (var lifetimeScope = container.BeginLifetimeScope(RequestLifetimeScopeProvider.HttpRequestTag))
            {
                var webViewPage = lifetimeScope.Resolve<GeneratedWebViewPage>();
                Assert.That(webViewPage, Is.Not.Null);
                Assert.That(webViewPage.Dependency, Is.Not.Null);
            }
        }

        [Test]
        public void RegistrationFoundForWebViewPageWithModel()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ViewDependency>().AsSelf();
            builder.RegisterSource(new ViewRegistrationSource());

            var container = builder.Build();
            using (var lifetimeScope = container.BeginLifetimeScope(RequestLifetimeScopeProvider.HttpRequestTag))
            {
                var webViewPage = lifetimeScope.Resolve<GeneratedWebViewPageWithModel>();
                Assert.That(webViewPage, Is.Not.Null);
                Assert.That(webViewPage.Dependency, Is.Not.Null);
            }
        }

        [Test]
        public void RegistrationFoundForViewPageWithoutModel()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ViewDependency>().AsSelf();
            builder.RegisterSource(new ViewRegistrationSource());

            var container = builder.Build();
            using (var lifetimeScope = container.BeginLifetimeScope(RequestLifetimeScopeProvider.HttpRequestTag))
            {
                var viewPage = lifetimeScope.Resolve<GeneratedViewPage>();
                Assert.That(viewPage, Is.Not.Null);
                Assert.That(viewPage.Dependency, Is.Not.Null);
            }
        }

        [Test]
        public void RegistrationFoundForViewPageWithModel()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ViewDependency>().AsSelf();
            builder.RegisterSource(new ViewRegistrationSource());

            var container = builder.Build();
            using (var lifetimeScope = container.BeginLifetimeScope(RequestLifetimeScopeProvider.HttpRequestTag))
            {
                var viewPage = lifetimeScope.Resolve<GeneratedViewPageWithModel>();
                Assert.That(viewPage, Is.Not.Null);
                Assert.That(viewPage.Dependency, Is.Not.Null);
            }
        }

        [Test]
        public void RegistrationFoundForViewMasterPageWithoutModel()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ViewDependency>().AsSelf();
            builder.RegisterSource(new ViewRegistrationSource());

            var container = builder.Build();
            using (var lifetimeScope = container.BeginLifetimeScope(RequestLifetimeScopeProvider.HttpRequestTag))
            {
                var viewMasterPage = lifetimeScope.Resolve<GeneratedViewMasterPage>();
                Assert.That(viewMasterPage, Is.Not.Null);
                Assert.That(viewMasterPage.Dependency, Is.Not.Null);
            }
        }

        [Test]
        public void RegistrationFoundForViewMasterPageWithModel()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ViewDependency>().AsSelf();
            builder.RegisterSource(new ViewRegistrationSource());

            var container = builder.Build();
            using (var lifetimeScope = container.BeginLifetimeScope(RequestLifetimeScopeProvider.HttpRequestTag))
            {
                var viewMasterPage = lifetimeScope.Resolve<GeneratedViewMasterPageWithModel>();
                Assert.That(viewMasterPage, Is.Not.Null);
                Assert.That(viewMasterPage.Dependency, Is.Not.Null);
            }
        }

        [Test]
        public void RegistrationFoundForViewUserControlWithoutModel()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ViewDependency>().AsSelf();
            builder.RegisterSource(new ViewRegistrationSource());

            var container = builder.Build();
            using (var lifetimeScope = container.BeginLifetimeScope(RequestLifetimeScopeProvider.HttpRequestTag))
            {
                var viewUserControl = lifetimeScope.Resolve<GeneratedViewUserControl>();
                Assert.That(viewUserControl, Is.Not.Null);
                Assert.That(viewUserControl.Dependency, Is.Not.Null);
            }
        }

        [Test]
        public void RegistrationFoundForViewUserControlWithModel()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ViewDependency>().AsSelf();
            builder.RegisterSource(new ViewRegistrationSource());

            var container = builder.Build();
            using (var lifetimeScope = container.BeginLifetimeScope(RequestLifetimeScopeProvider.HttpRequestTag))
            {
                var viewUserControl = lifetimeScope.Resolve<GeneratedViewUserControlWithModel>();
                Assert.That(viewUserControl, Is.Not.Null);
                Assert.That(viewUserControl.Dependency, Is.Not.Null);
            }
        }

        [Test]
        public void IsNotAdapterForIndividualComponents()
        {
            Assert.That(new ViewRegistrationSource().IsAdapterForIndividualComponents, Is.False);
        }

        [Test]
        public void ViewRegistrationIsInstancePerDependency()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ViewDependency>().AsSelf();
            builder.RegisterSource(new ViewRegistrationSource());

            var container = builder.Build();
            using (var lifetimeScope = container.BeginLifetimeScope(RequestLifetimeScopeProvider.HttpRequestTag))
            {
                var viewPage1 = lifetimeScope.Resolve<GeneratedWebViewPageWithModel>();
                var viewPage2 = lifetimeScope.Resolve<GeneratedWebViewPageWithModel>();

                Assert.That(viewPage1, Is.Not.SameAs(viewPage2));
            }
        }

        [Test]
        public void ViewCanHaveInstancePerHttpRequestDependency()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ViewDependency>().AsSelf().InstancePerHttpRequest();
            builder.RegisterSource(new ViewRegistrationSource());

            var container = builder.Build();
            using (var lifetimeScope = container.BeginLifetimeScope(RequestLifetimeScopeProvider.HttpRequestTag))
            {
                var viewPage1 = lifetimeScope.Resolve<GeneratedWebViewPageWithModel>();
                var viewPage2 = lifetimeScope.Resolve<GeneratedWebViewPageWithModel>();

                Assert.That(viewPage1.Dependency, Is.SameAs(viewPage2.Dependency));
            }
        }
    }

    public class ViewDependency { }

    public class ViewModel { }

    public abstract class AbstractWebViewPage : WebViewPage
    {
        public ViewDependency Dependency { get; set; }
    }

    public abstract class AbstractWebViewPageWithModel<T> : WebViewPage<T>
    {
        public ViewDependency Dependency { get; set; }
    }

    public class GeneratedWebViewPage : AbstractWebViewPage
    {
        public override void Execute() { }
    }

    public class GeneratedWebViewPageWithModel : AbstractWebViewPageWithModel<Model>
    {
        public override void Execute() { }
    }

    public abstract class AbstractViewPage : ViewPage
    {
        public ViewDependency Dependency { get; set; }
    }

    public abstract class AbstractViewPageWithModel<T> : ViewPage<T>
    {
        public ViewDependency Dependency { get; set; }
    }

    public class GeneratedViewPage : AbstractViewPage { }

    public class GeneratedViewPageWithModel : AbstractViewPageWithModel<Model> { }

    public abstract class AbstractViewMasterPage : ViewMasterPage
    {
        public ViewDependency Dependency { get; set; }
    }

    public abstract class AbstractViewMasterPageWithModel<T> : ViewMasterPage<T>
    {
        public ViewDependency Dependency { get; set; }
    }

    public class GeneratedViewMasterPage : AbstractViewMasterPage { }

    public class GeneratedViewMasterPageWithModel : AbstractViewMasterPageWithModel<Model> { }

    public abstract class AbstractViewUserControl : ViewUserControl
    {
        public ViewDependency Dependency { get; set; }
    }

    public abstract class AbstractViewUserControlWithModel<T> : ViewUserControl<T>
    {
        public ViewDependency Dependency { get; set; }
    }

    public class GeneratedViewUserControl : AbstractViewUserControl { }

    public class GeneratedViewUserControlWithModel : AbstractViewUserControlWithModel<Model> { }
}