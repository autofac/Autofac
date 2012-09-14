using System;
using System.Collections.Generic;
using System.Linq;

namespace Autofac.Tests.AppCert
{
    /// <summary>
    /// Simple date provider implementation.
    /// </summary>
    public class DateProvider : IDateProvider
    {
        /// <summary>
        /// Gets the current date.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> with the current local date/time.
        /// </value>
        public DateTime Now
        {
            get { return DateTime.Now; }
        }
    }
}
