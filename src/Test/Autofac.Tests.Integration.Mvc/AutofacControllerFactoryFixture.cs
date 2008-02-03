using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac.Integration.Mvc;
using System.Web.Mvc;
using System.Web;
using Autofac.Builder;

namespace Autofac.Tests.Integration.Mvc
{
    [TestFixture]
    public class AutofacControllerFactoryFixture
    {
        #region Stubs

        class StubController : Controller { }

        class StubContext : IHttpContext
        {
            #region IHttpContext Members

            public void AddError(Exception errorInfo)
            {
                throw new NotImplementedException();
            }

            public Exception[] AllErrors
            {
                get { throw new NotImplementedException(); }
            }

            public HttpApplicationState Application
            {
                get { throw new NotImplementedException(); }
            }

            public HttpApplication ApplicationInstance
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            public System.Web.Caching.Cache Cache
            {
                get { throw new NotImplementedException(); }
            }

            public void ClearError()
            {
                throw new NotImplementedException();
            }

            public IHttpHandler CurrentHandler
            {
                get { throw new NotImplementedException(); }
            }

            public RequestNotification CurrentNotification
            {
                get { throw new NotImplementedException(); }
            }

            public Exception Error
            {
                get { throw new NotImplementedException(); }
            }

            public object GetSection(string sectionName)
            {
                throw new NotImplementedException();
            }

            public IHttpHandler Handler
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            public bool IsCustomErrorEnabled
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsDebuggingEnabled
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsPostNotification
            {
                get { throw new NotImplementedException(); }
            }

            public System.Collections.IDictionary Items
            {
                get { throw new NotImplementedException(); }
            }

            public IHttpHandler PreviousHandler
            {
                get { throw new NotImplementedException(); }
            }

            public System.Web.Profile.ProfileBase Profile
            {
                get { throw new NotImplementedException(); }
            }

            public IHttpRequest Request
            {
                get { throw new NotImplementedException(); }
            }

            public IHttpResponse Response
            {
                get { throw new NotImplementedException(); }
            }

            public void RewritePath(string filePath, string pathInfo, string queryString, bool setClientFilePath)
            {
                throw new NotImplementedException();
            }

            public void RewritePath(string filePath, string pathInfo, string queryString)
            {
                throw new NotImplementedException();
            }

            public void RewritePath(string path, bool rebaseClientPath)
            {
                throw new NotImplementedException();
            }

            public void RewritePath(string path)
            {
                throw new NotImplementedException();
            }

            public IHttpServerUtility Server
            {
                get { throw new NotImplementedException(); }
            }

            public IHttpSessionState Session
            {
                get { throw new NotImplementedException(); }
            }

            public bool SkipAuthorization
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            public DateTime Timestamp
            {
                get { throw new NotImplementedException(); }
            }

            public TraceContext Trace
            {
                get { throw new NotImplementedException(); }
            }

            public System.Security.Principal.IPrincipal User
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            #endregion

            #region IServiceProvider Members

            public object GetService(Type serviceType)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        #endregion

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DetectsNullContext()
        {
            var target = new AutofacControllerFactory();
            target.CreateController(null, typeof(StubController));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DetectsNullControllerType()
        {
			IHttpContext httpContext = new StubContext();
			RequestContext context = new RequestContext(httpContext, new RouteData());

            var target = new AutofacControllerFactory();
            target.CreateController(context, null);
        }

        class ContainerProvidedAutofacControllerFactory : AutofacControllerFactory
        {
            public IContainer Container { get; set; }

            protected override IContainer ObtainContainer()
            {
                return Container;
            }
        }

        [Test]
        public void CreatesController()
        {
            var builder = new ContainerBuilder();
            builder.Register<StubController>().WithScope(InstanceScope.Factory);
            var container = builder.Build();

            var httpContext = new StubContext();
            var context = new RequestContext(httpContext, new RouteData());

            var target = new ContainerProvidedAutofacControllerFactory();
            target.Container = container;
            var controller = target.CreateController(context, typeof(StubController));

            Assert.IsNotNull(controller);
            Assert.IsInstanceOfType(typeof(StubController), controller);
        }
    }
}
