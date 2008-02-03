using System.Web.Mvc;
using System.Web;
using System;

namespace Autofac.Integration.Mvc
{
    /// <summary>
    /// Helper class to integrate Autofac IoC container into the MS-MVC web
    /// framework.
    /// </summary>
    public class AutofacMvcIntegration
    {
        private static IContainer _applicationContainer;

        /// <summary>
        /// Gets or sets the root container that holds registrations and singleton components
        /// shared between all requests in an application.
        /// </summary>
        /// <value>The application container.</value>
        public static IContainer ApplicationContainer
        {
            get
            {
                if (_applicationContainer == null)
                    throw new InvalidOperationException(AutofacMvcIntegrationResources.ContainerNotInstalled);

                return _applicationContainer;
            }
            private set
            {
                _applicationContainer = value;
            }
        }

        /// <summary>
        /// Gets the container that can be used to create and dispose of components used in a
        /// single web request (will defer resolution of singleton components to the ApplicationContainer
        /// container.
        /// </summary>
        /// <value>The request container.</value>
        public static IContainer RequestContainer
        {
            get
            {
                var result = NullableRequestContainer;
                if (result == null)
                    result = NullableRequestContainer = ApplicationContainer.CreateInnerContainer();
                
                return result;
            }
        }

        internal static IContainer NullableRequestContainer
        {
            get
            {
                return (IContainer)HttpContext.Current.Items[typeof(IContainer)];
            }
            set
            {
                HttpContext.Current.Items[typeof(IContainer)] = value;
            }
        }

        /// <summary>
        /// Installs the specified container as the root container of the application.
        /// </summary>
        /// <param name="applicationContainer">The application container.</param>
        public static void Install(IContainer applicationContainer)
        {
            if (applicationContainer == null)
                throw new ArgumentNullException("applicationContainer");

            ApplicationContainer = applicationContainer;
            ControllerBuilder.Current.SetDefaultControllerFactory(typeof(AutofacControllerFactory));
        }
    }
}
