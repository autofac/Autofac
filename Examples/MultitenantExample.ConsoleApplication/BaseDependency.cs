using System;

namespace MultitenantExample.ConsoleApplication
{
    /// <summary>
    /// Base class for dependencies. Used simply to avoid redundant code; it's not
    /// actually required to have a common derivation chain.
    /// </summary>
    public class BaseDependency : IDependency
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDependency"/> class.
        /// </summary>
        public BaseDependency()
        {
            this.InstanceId = Guid.NewGuid();
        }

        /// <summary>
        /// Gets the unique instance ID for the dependency.
        /// </summary>
        /// <value>
        /// A <see cref="System.Guid"/> that indicates the unique ID for the
        /// instance.
        /// </value>
        public Guid InstanceId { get; private set; }
    }
}
