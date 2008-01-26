using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Autofac.Integration.Web
{
    /// <summary>
    /// Provides application-wide and per-request containers.
    /// </summary>
    public class ContainerProvider : IContainerProvider
    {
        Container _applicationContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerProvider"/> class.
        /// </summary>
        /// <param name="applicationContainer">The application container.</param>
        public ContainerProvider(Container applicationContainer)
        {
            if (applicationContainer == null)
                throw new ArgumentNullException("applicationContainer");

            _applicationContainer = applicationContainer;
        }

        #region IContainerProvider Members

        /// <summary>
        /// Dispose of the current request's container, if it has been
        /// instantiated.
        /// </summary>
        public void DisposeRequestContainer()
        {
            var rc = NullableRequestContainer;
            if (rc != null)
                rc.Dispose();
        }

        /// <summary>
        /// The global, application-wide container.
        /// </summary>
        /// <value></value>
        public Container ApplicationContainer
        {
            get
            {
                return _applicationContainer;
            }
        }

        /// <summary>
        /// The container used to manage components for processing the
        /// current request.
        /// </summary>
        /// <value></value>
        public Container RequestContainer
        {
            get
            {
                var result = NullableRequestContainer;
                if (result == null)
                    result = NullableRequestContainer = ApplicationContainer.CreateInnerContainer();

                return result;
            }
        }

        #endregion

        Container NullableRequestContainer
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
    }
}
