using System;
using System.Collections.Generic;
using System.Linq;

namespace Autofac.Core
{
    /// <summary>
    /// Service used as a "flag" to indicate a particular component should be
    /// autostarted on container build.
    /// </summary>
    internal class AutoStartService : Service
    {
        /// <summary>
        /// Gets the service description.
        /// </summary>
        /// <value>
        /// Always returns <c>AutoStart</c>.
        /// </value>
        public override string Description
        {
            get { return "AutoStart"; }
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="System.Object"/>.</param>
        /// <returns>
        /// <see langword="true" /> if the specified <see cref="System.Object"/> is not <see langword="null" />
        /// and is an <see cref="Autofac.Core.AutoStartService"/>; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        /// <para>
        /// All services of this type are considered "equal."
        /// </para>
        /// </remarks>
        public override bool Equals(object obj)
        {
            AutoStartService that = obj as AutoStartService;
            return that != null;
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>. Always <c>0</c> for this type.
        /// </returns>
        /// <remarks>
        /// <para>
        /// All services of this type are considered "equal" and use the same hash code.
        /// </para>
        /// </remarks>
        public override int GetHashCode()
        {
            return 0;
        }
    }
}
