using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using Autofac;

namespace MvcShareTrader
{
    public class AutofacHttpModule : IHttpModule
    {
        public static Container Container { get; set; }

        public static Container RequestContainer
        {
            get
            {
                var result = (Container)HttpContext.Current.Items[typeof(Container)];
                if (result == null)
                {
                    result = RequestContainer = Container.CreateInnerContainer();
                    RequestContainer = result;
                }
                return result;
            }
            private set
            {
                HttpContext.Current.Items[typeof(Container)] = value;
            }
        }
        
        #region IHttpModule Members

        public void Dispose()
        {
            Container.Dispose();
        }

        public void Init(HttpApplication context)
        {
            context.EndRequest += Application_EndRequest;
        }

        #endregion

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            RequestContainer.Dispose();
        }
    }
}
