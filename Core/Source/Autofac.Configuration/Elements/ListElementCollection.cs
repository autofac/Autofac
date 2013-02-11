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
using Autofac.Configuration.Util;

namespace Autofac.Configuration.Elements
{
    /// <summary>
    /// Holds a list of values for those properties/parameters that are enumerable
    /// </summary>
    [TypeConverter(typeof(ListElementTypeConverter))]
    public class ListElementCollection : ConfigurationElementCollection<ListItemElement>
    {
        /// <summary>
        /// Helps convert the configuration element into an actuall generic list
        /// </summary>
        private class ListElementTypeConverter : TypeConverter
        {
            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                var instantiatableType = GetInstantiableType(destinationType);

                var castValue = value as ListElementCollection;
                if (castValue != null &&
                    instantiatableType != null)
                {
                    Type[] generics = instantiatableType.GetGenericArguments();

                    var collection = (IList)Activator.CreateInstance(instantiatableType);
                    foreach (var item in castValue)
                    {
                        collection.Add(TypeManipulation.ChangeToCompatibleType(item.Value, generics[0]));
                    }
                    return collection;
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
                if (typeof(IEnumerable).IsAssignableFrom(destinationType))
                {
                    Type[] generics = destinationType.IsGenericType ? destinationType.GetGenericArguments() : new[] { typeof(object) };
                    if (generics.Length != 1)
                        return null;

                    Type listType = typeof(List<>).MakeGenericType(generics);

                    //can we assign this?
                    if (destinationType.IsAssignableFrom(listType))
                        return listType;
                }

                return null;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ListElementCollection"/> class.
        /// </summary>
        public ListElementCollection() : base("item")
        {
        }
    }
}