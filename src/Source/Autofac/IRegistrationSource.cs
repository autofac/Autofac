using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac
{
    /// <summary>
    /// Allows registrations to be made on-the-fly when unregistered
	/// services are requested (lazy registrations.)
    /// </summary>
    public interface IRegistrationSource
    {
		/// <summary>
		/// Retrieve a registration for an unregistered service, to be used
		/// by the container.
		/// </summary>
		/// <param name="service">The service that was requested.</param>
		/// <param name="registration">A registration providing the service.</param>
		/// <returns>True if the registration could be created.</returns>
		bool TryGetRegistration(Service service, out IComponentRegistration registration);
    }
}
