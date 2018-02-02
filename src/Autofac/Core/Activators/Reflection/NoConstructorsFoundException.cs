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
        /// <param name="type">The type whose constructor was not found.</param>
        public NoConstructorsFoundException(Type type)
            : this(type, FormatMessage(type))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoConstructorsFoundException"/> class.
        /// </summary>
        /// <param name="type">The type whose constructor was not found.</param>
        /// <param name="message">Exception message.</param>
        public NoConstructorsFoundException(Type type, string message)
            : base(message)
        {
            this.OffendingType = type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoConstructorsFoundException"/> class.
        /// </summary>
        /// <param name="type">The type whose constructor was not found.</param>
        /// <param name="innerException">The inner exception.</param>
        public NoConstructorsFoundException(Type type, Exception innerException)
            : this(type, FormatMessage(type), innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoConstructorsFoundException"/> class.
        /// </summary>
        /// <param name="type">The type whose constructor was not found.</param>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public NoConstructorsFoundException(Type type, string message, Exception innerException)
            : base(message, innerException)
        {
            this.OffendingType = type;
        }

        public Type OffendingType { get; private set; }

        private static string FormatMessage(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return string.Format(CultureInfo.CurrentCulture, NoConstructorsFoundExceptionResources.Message, type.FullName);
        }
    }
}
