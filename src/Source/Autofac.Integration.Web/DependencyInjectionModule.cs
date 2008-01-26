using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Autofac.Integration.Web
{
    /// <summary>
    /// Injects dependencies into request handlers that have been
    /// decorated with the [InjectProperties] or [InjectUnsetProperties]
    /// attributes.
    /// </summary>
    public class DependencyInjectionModule : IHttpModule
    {
        IContainerProviderAccessor _containerProviderAccessor;
        HttpApplication _httpApplication;
        IInjectionBehaviour _noInjection = new NoInjection();
        IInjectionBehaviour _injectProperties = new PropertyInjection();
        IInjectionBehaviour _injectUnsetProperties = new UnsetPropertyInjection();

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

            _httpApplication = context;
            _containerProviderAccessor = context as IContainerProviderAccessor;

            if (_containerProviderAccessor == null)
                throw new InvalidOperationException("App must support acc.");

            context.PreRequestHandlerExecute += OnPreRequestHandlerExecute;
        }

        #endregion

        /// <summary>
        /// Called before the request handler is executed so that dependencies
        /// can be injected.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void OnPreRequestHandlerExecute(object sender, EventArgs e)
        {
            var handler = _httpApplication.Context.CurrentHandler;
            var injectionBehaviour = GetInjectionBehaviour(handler);
            injectionBehaviour.Inject(_containerProviderAccessor.ContainerProvider.RequestContainer, handler);
        }

        private IInjectionBehaviour GetInjectionBehaviour(IHttpHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");

            var result = _noInjection;

            if (!(handler is DefaultHttpHandler))
            {
                var handlerType = handler.GetType();

                if (handlerType.GetCustomAttributes(typeof(InjectPropertiesAttribute), true).Length > 0)
                {
                    result = _injectProperties;
                }
                else if (handlerType.GetCustomAttributes(typeof(InjectUnsetPropertiesAttribute), true).Length > 0)
                {
                    result = _injectUnsetProperties;
                }
            }

            return result;
        }
    }
}
