// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Globalization;

namespace Autofac.Core
{
    /// <summary>
    /// Base exception type thrown whenever the dependency resolution process fails. This is a fatal
    /// exception, as Autofac is unable to 'roll back' changes to components that may have already
    /// been made during the operation. For example, 'on activated' handlers may have already been
    /// fired, or 'single instance' components partially constructed.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public class DependencyResolutionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyResolutionException"/> class.
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

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <value>
        /// The error message that explains the reason for the exception, or an empty string("").
        /// </value>
        public override string Message
        {
            get
            {
                // Issue 343: Including the inner exception message with the
                // main message for easier debugging.
                var message = base.Message;
                if (InnerException == null)
                    return message;

                var inner = InnerException.Message;
                message = string.Format(CultureInfo.CurrentCulture, DependencyResolutionExceptionResources.MessageNestingFormat, message, inner);
                return message;
            }
        }
    }
}
