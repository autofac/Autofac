using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Autofac.Configuration
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
        public class ListElementTypeConverter : TypeConverter
        {
            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                if (value is ListElementCollection &&
                    CanConvertTo(context, destinationType))
                {
                    Type[] generics = destinationType.IsGenericType ? destinationType.GetGenericArguments() : new[] { typeof(object) };
                    Type listType = typeof(List<>).MakeGenericType(generics);

                    var collection = (IList)Activator.CreateInstance(listType);
                    foreach (var item in (ListElementCollection)value)
                    {
                        collection.Add(TypeManipulation.ChangeToCompatibleType(item.Value, generics[0]));
                    }
                    return collection;
                }

                return base.ConvertTo(context, culture, value, destinationType);
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                if (typeof(IEnumerable).IsAssignableFrom(destinationType))
                {
                    Type[] generics = destinationType.IsGenericType ? destinationType.GetGenericArguments() : new[] { typeof(object) };
                    if (generics.Length != 1)
                        return false;

                    Type listType = typeof(List<>).MakeGenericType(generics);

                    //can we assign this?
                    return destinationType.IsAssignableFrom(listType);
                }

                return base.CanConvertTo(context, destinationType);
            }
        }

        public ListElementCollection() 
            : base("item")
        {
        }
    }
}