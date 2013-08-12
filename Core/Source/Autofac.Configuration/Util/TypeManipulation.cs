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
using System.Linq;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Reflection;

namespace Autofac.Configuration.Util
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
        /// <param name="memberInfo">Reflected property or member info for the destination, if available, for retrieving custom type converter information.</param>
        /// <returns>An object of the destination type.</returns>
        public static object ChangeToCompatibleType(object value, Type destinationType, ICustomAttributeProvider memberInfo)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }

            if (value == null)
            {
                return destinationType.IsValueType ? Activator.CreateInstance(destinationType) : null;
            }

            TypeConverter converter = null;
            if (memberInfo != null)
            {
                // Try to get custom type converter information.
                var attrib = memberInfo.GetCustomAttributes(typeof(TypeConverterAttribute), true).Cast<TypeConverterAttribute>().FirstOrDefault();
                if (attrib != null && !String.IsNullOrEmpty(attrib.ConverterTypeName))
                {
                    converter = GetTypeConverterFromName(attrib.ConverterTypeName);
                    if (converter.CanConvertFrom(value.GetType()))
                    {
                        return converter.ConvertFrom(value);
                    }
                }
            }

            // If there's not a custom converter specified via attribute, try for a default.
            converter = TypeDescriptor.GetConverter(value.GetType());
            if (converter.CanConvertTo(destinationType))
            {
                return converter.ConvertTo(value, destinationType);
            }

            // Try implicit conversion.
            if (destinationType.IsInstanceOfType(value))
            {
                return value;
            }

            // Try explicit opposite conversion.
            converter = TypeDescriptor.GetConverter(destinationType);
            if (converter.CanConvertFrom(value.GetType()))
            {
                return converter.ConvertFrom(value);
            }

            // Try a TryParse method.
            if (value is string)
            {
                var parser = destinationType.GetMethod("TryParse", BindingFlags.Static | BindingFlags.Public);
                if (parser != null)
                {
                    var parameters = new[] { value, null };
                    if ((bool)parser.Invoke(null, parameters))
                    {
                        return parameters[1];
                    }
                }
            }

            throw new ConfigurationErrorsException(String.Format(CultureInfo.CurrentCulture, ConfigurationSettingsReaderResources.TypeConversionUnsupported, value.GetType(), destinationType));
        }

        private static TypeConverter GetTypeConverterFromName(string converterTypeName)
        {
            var converterType = Type.GetType(converterTypeName, true);
            var converter = Activator.CreateInstance(converterType) as TypeConverter;
            if (converter == null)
            {
                throw new ConfigurationErrorsException(String.Format(CultureInfo.CurrentCulture, ConfigurationSettingsReaderResources.TypeConverterAttributeTypeNotConverter, converterTypeName));
            }
            return converter;
        }
    }
}
