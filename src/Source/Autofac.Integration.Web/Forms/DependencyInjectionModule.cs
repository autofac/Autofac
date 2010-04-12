// This software is part of the Autofac IoC container
// Copyright (c) 2010 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Web;

namespace Autofac.Integration.Web.Forms
{
    /// <summary>
    /// Base for classes that inject dependencies into HTTP Handlers.
    /// </summary>
    public abstract class DependencyInjectionModule : IHttpModule
    {
        IContainerProviderAccessor _containerProviderAccessor;
        HttpApplication _httpApplication;
        IInjectionBehaviour _noInjection = new NoInjection();
        IInjectionBehaviour _propertyInjection = new PropertyInjection();
        IInjectionBehaviour _unsetPropertyInjection = new UnsetPropertyInjection();

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
                throw new InvalidOperationException(DependencyInjectionModuleResources.ApplicationMustImplementAccessor);

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
            if (handler != null)
            {
                var injectionBehaviour = GetInjectionBehaviour(handler);
                var cp = _containerProviderAccessor.ContainerProvider;
                if (cp == null)
                    throw new InvalidOperationException(ContainerDisposalModuleResources.ContainerProviderNull);
                injectionBehaviour.InjectDependencies(cp.RequestLifetime, handler);
            }
        }

        /// <summary>
        /// Internal for testability outside of a web application.
        /// </summary>
        /// <param name="handler"></param>
        /// <returns>The injection behaviour.</returns>
        protected internal IInjectionBehaviour GetInjectionBehaviour(IHttpHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");

            if (handler is DefaultHttpHandler)
            {
                return _noInjection;
            }
            else
            {
                var handlerType = handler.GetType();
                return GetInjectionBehaviourForHandlerType(handlerType);
            }
        }

        /// <summary>
        /// A behaviour that does not inject dependencies.
        /// </summary>
        protected IInjectionBehaviour NoInjection
        {
            get { return _noInjection; }
        }

        /// <summary>
        /// A behaviour that injects resolvable dependencies.
        /// </summary>
        protected IInjectionBehaviour PropertyInjection
        {
            get { return _propertyInjection; }
        }

        /// <summary>
        /// A behaviour that injects unset, resolvable dependencies.
        /// </summary>
        protected IInjectionBehaviour UnsetPropertyInjection
        {
            get { return _unsetPropertyInjection; }
        }

        /// <summary>
        /// Override to customise injection behaviour based on HTTP Handler type.
        /// </summary>
        /// <param name="handlerType">Type of the handler.</param>
        /// <returns>The injection behaviour.</returns>
        protected abstract IInjectionBehaviour GetInjectionBehaviourForHandlerType(Type handlerType);
    }
}


