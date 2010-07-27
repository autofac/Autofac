using System;

namespace MultitenantExample.MvcApplication.Dependencies
{
    /// <summary>
    /// Demonstration dependency interface that allows you to inspect the unique
    /// ID on a specific resolved instance of the dependency.
    /// </summary>
    public interface IDependency
    {
        /// <summary>
        /// Gets the unique instance ID for the dependency.
        /// </summary>
        /// <value>
        /// A <see cref="System.Guid"/> that indicates the unique ID for the
        /// instance.
        /// </value>
        Guid InstanceId { get; }
    }
}
