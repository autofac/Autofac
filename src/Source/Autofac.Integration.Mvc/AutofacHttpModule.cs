using System;
using System.Web;

namespace Autofac.Integration.Mvc
{
    /// <summary>
    /// HTTP Module that disposes of Autofac-created components when processing for
    /// a request completes.
    /// </summary>
    public class AutofacHttpModule : IHttpModule
    {
        #region IHttpModule Members

        /// <summary>
        /// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule"/>.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpApplication"/> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application</param>
        public void Init(HttpApplication context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            context.EndRequest += OnEndRequest;
        }

        #endregion

        void OnEndRequest(object sender, EventArgs e)
        {
            var requestContainer = AutofacMvcIntegration.NullableRequestContainer;
            if (requestContainer != null)
                requestContainer.Dispose();
        }
    }
}
