// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2008 Autofac Contributors
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
using System.ComponentModel;

namespace Autofac.Configuration
{
    /// <summary>
    /// Some handy type conversion routines.
    /// </summary>
    class TypeManipulation
    {
        /// <summary>
        /// Does its best to convert whatever the value is into the destination
        /// type. Null in yields null out for value types and the default(T)
        /// for value types (this may change.)
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="destinationType">Type of the destination.</param>
        /// <returns>An object of the destination type.</returns>
        public static object ChangeToCompatibleType(object value, Type destinationType)
        {
            Enforce.ArgumentNotNull(destinationType, "destinationType");

            if (value == null)
            {
                if (destinationType.IsValueType)
                    return Activator.CreateInstance(destinationType);

                return null;
            }

            if (destinationType.IsAssignableFrom(value.GetType()))
                return value;

            return TypeDescriptor.GetConverter(destinationType).ConvertFrom(value);
        }
    }
}
