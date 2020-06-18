using System;
using System.Collections.Generic;
using System.Text;

namespace Autofac.Core.Registration
{
    /// <summary>
    /// Defines options for a registration.
    /// </summary>
    [Flags]
    public enum RegistrationOptions
    {
        /// <summary>
        /// No special options; default behaviour.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates that this registration is 'fixed' as the default, ignoring all other registrations when determining the default registration for
        /// a service.
        /// </summary>
        Fixed = 2,

        /// <summary>
        /// Registrations with this flag will not be decorated.
        /// </summary>
        DisableDecoration = 4,

        /// <summary>
        /// Registrations with this flag will not be included in any collection resolves (i.e. <see cref="IEnumerable{TService}" /> and other collection types).
        /// </summary>
        ExcludeFromCollections = 8,

        /// <summary>
        /// Flag combination for composite registrations.
        /// </summary>
        Composite = Fixed | DisableDecoration | ExcludeFromCollections,
    }
}
