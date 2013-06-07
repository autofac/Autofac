using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace Autofac.Extras.Attributed
{
    /// <summary>
    /// Translates a type's attribute properties into a set consumable by Autofac.
    /// </summary>
    public static class MetadataHelper
    {
        /// <summary>
        /// Given a target object, returns a set of properties and associated values.
        /// </summary>
        /// <param name="target">Target instance to be scanned.</param>
        /// <returns>Enumerable set of properties and associated values.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="target" /> is <see langword="null" />.
        /// </exception>
        public static IEnumerable<KeyValuePair<string, object>> GetProperties(object target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }
            return target.GetType()
                         .GetProperties()
                         .Where(propertyInfo => propertyInfo.CanRead &&
                                propertyInfo.DeclaringType != null &&
                                propertyInfo.DeclaringType.Name != typeof(Attribute).Name)
                         .Select(propertyInfo =>
                                  new KeyValuePair<string, object>(propertyInfo.Name, propertyInfo.GetValue(target, null)));
        }


        /// <summary>
        /// Given a type, interrogate the attribution to retrieve an enumerable set property names.
        /// </summary>
        /// <param name="targetType">Type to interrogate for metdata attribute attributes.</param>
        /// <returns>Enumerable set of properties found.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="targetType" /> is <see langword="null" />.
        /// </exception>
        public static IEnumerable<KeyValuePair<string, object>> GetMetadata(Type targetType)
        {
            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }
            var propertyList = new List<KeyValuePair<string, object>>();

            foreach (var attribute in targetType.GetCustomAttributes(true)
                                                .Where(p => p.GetType().GetCustomAttributes(typeof(MetadataAttributeAttribute), true).Any()))
                propertyList.AddRange(GetProperties(attribute));

            return propertyList;
        }

        /// <summary>
        /// Given a strong type, interrogate the attribution to retrieve an enumerable set of property names.
        /// </summary>
        /// <typeparam name="TMetadataType">Metadata type to look for in the list of attributes.</typeparam>
        /// <param name="targetType">Type to interrogate.</param>
        /// <returns>Enumerable set of properties found.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="targetType" /> is <see langword="null" />.
        /// </exception>
        public static IEnumerable<KeyValuePair<string, object>> GetMetadata<TMetadataType>(Type targetType)
        {
            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }
            var attribute =
                (from p in targetType.GetCustomAttributes(typeof(TMetadataType), true) select p).FirstOrDefault();

            return attribute != null ? GetProperties(attribute) : new List<KeyValuePair<string, object>>();
        }
    }
}
