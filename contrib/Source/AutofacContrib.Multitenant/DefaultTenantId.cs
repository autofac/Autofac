using System;

namespace AutofacContrib.Multitenant
{
    /// <summary>
    /// Object type that represents a default tenant ID. Primarily used internally;
    /// most applications will not use this type.
    /// </summary>
    public class DefaultTenantId
    {
        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Properties.Resources.DefaultTenantId_ToString;
        }
    }
}
