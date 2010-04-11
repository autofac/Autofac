using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Autofac.Integration.Web.Mvc
{
    /// <summary>
    /// Registers http context abstraction classes including <see cref="HttpContextBase"/> and <see cref="HttpSessionStateBase "/>
    /// for use by components that live in the Request lifetime
    /// </summary>
    public class HttpModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => new HttpContextWrapper(HttpContext.Current))
                .As<HttpContextBase>()
                .HttpRequestScoped();

            builder.Register(c => c.Resolve<HttpContextBase>().Session)
                .As<HttpSessionStateBase>()
                .HttpRequestScoped();
        }

    }
}
