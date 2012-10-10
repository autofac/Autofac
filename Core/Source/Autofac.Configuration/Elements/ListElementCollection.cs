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

        public ListElementCollection()
            : base("item")
        {
        }
    }
}