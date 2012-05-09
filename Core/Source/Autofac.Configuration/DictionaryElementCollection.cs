using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;

namespace Autofac.Configuration
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
        public class DictionaryElementTypeConverter : TypeConverter
        {
            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                var instantiatableType = GetInstantiableType(destinationType);

                if (value is DictionaryElementCollection &&
                    instantiatableType != null)
                {
                    var dictionary = (IDictionary)Activator.CreateInstance(instantiatableType);
                    Type[] generics = instantiatableType.GetGenericArguments();
                    
                    foreach (var item in (DictionaryElementCollection)value)
                    {
                        if (String.IsNullOrEmpty(item.Key))
                            throw new ConfigurationErrorsException("Key cannot be null in a dictionary element.");

                        var convertedKey = TypeManipulation.ChangeToCompatibleType(item.Key, generics[0]);
                        var convertedValue = TypeManipulation.ChangeToCompatibleType(item.Value, generics[1]);

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

        public DictionaryElementCollection() 
            : base("item")
        {
        }
    }
}