// This software is part of the Autofac IoC container
// Copyright � 2018 Autofac Contributors
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

namespace Autofac.Core.Activators.Reflection
{
    public class NoConstructorsFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoConstructorsFoundException"/> class.
        /// </summary>
        /// <param name="offendingType">The <see cref="System.Type"/> whose constructor was not found.</param>
        public NoConstructorsFoundException(Type offendingType)
            : this(offendingType, FormatMessage(offendingType))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoConstructorsFoundException"/> class.
        /// </summary>
        /// <param name="offendingType">The <see cref="System.Type"/> whose constructor was not found.</param>
        /// <param name="message">Exception message.</param>
        public NoConstructorsFoundException(Type offendingType, string message)
            : base(message)
        {
            if (offendingType == null) throw new ArgumentNullException(nameof(offendingType));
            this.OffendingType = offendingType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoConstructorsFoundException"/> class.
        /// </summary>
        /// <param name="offendingType">The <see cref="System.Type"/> whose constructor was not found.</param>
        /// <param name="innerException">The inner exception.</param>
        public NoConstructorsFoundException(Type offendingType, Exception innerException)
            : this(offendingType, FormatMessage(offendingType), innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoConstructorsFoundException"/> class.
        /// </summary>
        /// <param name="offendingType">The <see cref="System.Type"/> whose constructor was not found.</param>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public NoConstructorsFoundException(Type offendingType, string message, Exception innerException)
            : base(message, innerException)
        {
            if (offendingType == null) throw new ArgumentNullException(nameof(offendingType));
            this.OffendingType = offendingType;
        }

        /// <summary>
        /// Gets the type without found constructors.
        /// </summary>
        /// <value>
        /// A <see cref="System.Type"/> that was processed by an <see cref="IConstructorFinder"/>
        /// or similar mechanism and was determined to have no available constructors.
        /// </value>
        public Type OffendingType { get; private set; }

        private static string FormatMessage(Type offendingType)
        {
            if (offendingType == null) throw new ArgumentNullException(nameof(offendingType));

            return string.Format(CultureInfo.CurrentCulture, NoConstructorsFoundExceptionResources.Message, offendingType.FullName);
        }
    }
}
