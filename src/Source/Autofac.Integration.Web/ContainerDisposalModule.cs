using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Autofac.Integration.Web
{
    /// <summary>
    /// HTTP Module that disposes of Autofac-created components when processing for
    /// a request completes.
    /// </summary>
    public class ContainerDisposalModule : IHttpModule
    {
        IContainerProviderAccessor _containerProviderAccessor;
        HttpApplication _httpApplication;

        #region IHttpModule Members

        /// <summary>
        /// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule"/>.
        /// </summary>
        public void Dispose()
        {
            _httpApplication.EndRequest -= OnEndRequest;
        }

        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpApplication"/> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application</param>
        public void Init(HttpApplication context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            _httpApplication = context;

            _containerProviderAccessor = context as IContainerProviderAccessor;
            if (_containerProviderAccessor == null)
                throw new InvalidOperationException(ContainerDisposalModuleResources.ApplicationMustImplementAccessor);

            context.EndRequest += OnEndRequest;
        }

        #endregion

        /// <summary>
        /// Dispose of the per-request container.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnEndRequest(object sender, EventArgs e)
        {
            _containerProviderAccessor.ContainerProvider.DisposeRequestContainer();
        }
    }
}
