using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Web;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.Configuration;

namespace EnterpriseLibraryExample.MvcApplication
{
    /// <summary>
    /// Custom exception handler that redirects the user to a fixed URL.
    /// </summary>
    [ConfigurationElementType(typeof(CustomHandlerData))]
    public class UrlRedirectHandler : IExceptionHandler
    {
        private string _url = null;

        public UrlRedirectHandler(NameValueCollection configuration)
        {
            if (configuration != null)
            {
                this.ProcessConfiguration(configuration);
            }
        }

        public Exception HandleException(Exception exception, Guid handlingInstanceId)
        {
            if (exception == null)
            {
                return null;
            }
            var context = HttpContext.Current;
            if (context == null)
            {
                return exception;
            }
            context.ClearError();
            context.Response.Redirect(this._url, true);
            return exception;
        }

        private void ProcessConfiguration(NameValueCollection configuration)
        {
            this._url = configuration["url"];
            if (String.IsNullOrEmpty(this._url))
            {
                throw new ConfigurationErrorsException("You must provide a URL to the URL redirect handler.");
            }
        }
    }
}