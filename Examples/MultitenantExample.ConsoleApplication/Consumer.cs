using System;

namespace MultitenantExample.ConsoleApplication
{
    /// <summary>
    /// Simple dependency consumer that takes a dependency as a constructor parameter.
    /// </summary>
    public class Consumer : IDependencyConsumer
    {
        /// <summary>
        /// Gets the dependency for this consumer.
        /// </summary>
        /// <value>
        /// An <see cref="MultitenantExample.ConsoleApplication.IDependency"/>
        /// held by this specific consumer.
        /// </value>
        public IDependency Dependency { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultitenantExample.ConsoleApplication.Consumer"/> class.
        /// </summary>
        /// <param name="dependency">The dependency this class consumes.</param>
        public Consumer(IDependency dependency)
        {
            this.Dependency = dependency;
        }
    }
}
