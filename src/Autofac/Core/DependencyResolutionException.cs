// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.Serialization;

namespace Autofac.Core
{
    /// <summary>
    /// Base exception type thrown whenever the dependency resolution process fails. This is a fatal
    /// exception, as Autofac is unable to 'roll back' changes to components that may have already
    /// been made during the operation. For example, 'on activated' handlers may have already been
    /// fired, or 'single instance' components partially constructed.
    /// </summary>
    [Serializable]
    public class DependencyResolutionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyResolutionException"/> class.
        /// </summary>
        /// <param name="info">The serialisation info.</param>
        /// <param name="context">The serialisation streaming context.</param>
        protected DependencyResolutionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyResolutionException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public DependencyResolutionException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyResolutionException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public DependencyResolutionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
