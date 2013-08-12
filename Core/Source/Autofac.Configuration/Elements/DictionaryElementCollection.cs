// This software is part of the Autofac IoC container
// Copyright (c) 2013 Autofac Contributors
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using Autofac.Configuration.Util;

namespace Autofac.Configuration.Elements
{
    /// <summary>
    /// Holds a dictionary of values for those properties/parameters that are a dictionary
    /// </summary>
    [TypeConverter(typeof(DictionaryElementTypeConverter))]
    public class DictionaryElementCollection : ConfigurationElementCollection<ListItemElement>
    {
        /// <summary>
        /// Helps convert the configuration element into an actuall generic list
        /// </summary>
        private class DictionaryElementTypeConverter : TypeConverter
        {
            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                var instantiatableType = GetInstantiableType(destinationType);

                var castValue = value as DictionaryElementCollection;
                if (castValue != null &&
                    instantiatableType != null)
                {
                    var dictionary = (IDictionary)Activator.CreateInstance(instantiatableType);
                    Type[] generics = instantiatableType.GetGenericArguments();

                    foreach (var item in castValue)
                    {
                        if (String.IsNullOrEmpty(item.Key))
                            throw new ConfigurationErrorsException("Key cannot be null in a dictionary element.");

                        var convertedKey = TypeManipulation.ChangeToCompatibleType(item.Key, generics[0], null);
                        var convertedValue = TypeManipulation.ChangeToCompatibleType(item.Value, generics[1], null);

                        dictionary.Add(convertedKey, convertedValue);
                    }
                    return dictionary;
                }

                return base.ConvertTo(context, culture, value, destinationType);
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                if (GetInstantiableType(destinationType) != null)
                    return true;

                return base.CanConvertTo(context, destinationType);
            }

            private static Type GetInstantiableType(Type destinationType)
            {
                if (typeof(IDictionary).IsAssignableFrom(destinationType)
                    || (destinationType.IsGenericType
                    && typeof(IDictionary<,>).IsAssignableFrom(destinationType.GetGenericTypeDefinition())))
                {
                    Type[] generics = destinationType.IsGenericType ? destinationType.GetGenericArguments() : new[] { typeof(string), typeof(object) };
                    if (generics.Length != 2)
                        return null;

                    Type dictType = typeof(Dictionary<,>).MakeGenericType(generics);

                    //can we assign this?
                    if (destinationType.IsAssignableFrom(dictType))
                        return dictType;
                }

                return null;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryElementCollection"/> class.
        /// </summary>
        public DictionaryElementCollection()
            : base("item")
        {
        }
    }
}