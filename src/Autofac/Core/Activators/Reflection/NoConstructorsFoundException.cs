// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;

namespace Autofac.Core.Activators.Reflection
{
    /// <summary>
    /// Exception thrown when no suitable constructors could be found on a type.
    /// </summary>
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
            OffendingType = offendingType ?? throw new ArgumentNullException(nameof(offendingType));
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
            OffendingType = offendingType ?? throw new ArgumentNullException(nameof(offendingType));
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
            if (offendingType == null)
            {
                throw new ArgumentNullException(nameof(offendingType));
            }

            return string.Format(CultureInfo.CurrentCulture, NoConstructorsFoundExceptionResources.Message, offendingType.FullName);
        }
    }
}
