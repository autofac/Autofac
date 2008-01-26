using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Integration.Web
{
    /// <summary>
    /// Provides global and per-request Autofac containers in an
    /// ASP.NET application.
    /// </summary>
    public interface IContainerProvider
    {
        /// <summary>
        /// Dispose of the current request's container, if it has been
        /// instantiated.
        /// </summary>
        void DisposeRequestContainer();

        /// <summary>
        /// The global, application-wide container.
        /// </summary>
        Container ApplicationContainer { get; }

        /// <summary>
        /// The container used to manage components for processing the
        /// current request.
        /// </summary>
        Container RequestContainer { get; }
    }
}
