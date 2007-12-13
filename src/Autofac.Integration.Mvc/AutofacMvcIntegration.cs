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
        private static Container _applicationContainer;

        /// <summary>
        /// Gets or sets the root container that holds registrations and singleton components
        /// shared between all requests in an application.
        /// </summary>
        /// <value>The application container.</value>
        public static Container ApplicationContainer
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
        public static Container RequestContainer
        {
            get
            {
                var result = NullableRequestContainer;
                if (result == null)
                    result = NullableRequestContainer = ApplicationContainer.CreateInnerContainer();
                
                return result;
            }
        }

        internal static Container NullableRequestContainer
        {
            get
            {
                return (Container)HttpContext.Current.Items[typeof(Container)];
            }
            set
            {
                HttpContext.Current.Items[typeof(Container)] = value;
            }
        }

        /// <summary>
        /// Installs the specified container as the root container of the application.
        /// </summary>
        /// <param name="applicationContainer">The application container.</param>
        public static void Install(Container applicationContainer)
        {
            if (applicationContainer == null)
                throw new ArgumentNullException("applicationContainer");

            ApplicationContainer = applicationContainer;
            ControllerBuilder.Current.SetDefaultControllerFactory(typeof(AutofacControllerFactory));
        }
    }
}
