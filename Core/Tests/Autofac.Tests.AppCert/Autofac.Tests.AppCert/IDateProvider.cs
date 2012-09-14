using System;
using System.Collections.Generic;
using System.Linq;

namespace Autofac.Tests.AppCert
{
    /// <summary>
    /// Simple provider service that gets the current date.
    /// </summary>
    public interface IDateProvider
    {
        /// <summary>
        /// Gets the current date.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> with the current local date/time.
        /// </value>
        DateTime Now { get; }
    }
}
