using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Builder
{
    /// <summary>
    /// Fired when a registrar is applied to a container.
    /// </summary>
    public class RegisteredEventArgs : EventArgs
    {
        /// <summary>
        /// The container.
        /// </summary>
        public IContainer Container { get; set; }

        /// <summary>
        /// The registration being made. May be null.
        /// </summary>
        public IComponentRegistration Registration { get; set; }
    }
}
