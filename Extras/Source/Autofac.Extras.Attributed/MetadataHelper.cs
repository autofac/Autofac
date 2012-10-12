using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace Autofac.Extras.Attributed
{
    /// <summary>
    /// this class is responsible for translating a types attribute properties into a set consumable by autofac
    /// </summary>
    public static class MetadataHelper
    {
        /// <summary>
        /// given a target object, returns a set of properties and associated values
        /// </summary>
        /// <param name="target">target instance to be scanned</param>
        /// <returns>enumerable set of properties and associated values</returns>
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
        /// given a type, interrogate the attribution to retrieve an enumerable set property names
        /// </summary>
        /// <param name="targetType">type to interrogate for metdata attribute attributes</param>
        /// <returns>enumerable set of properties found</returns>
        public static IEnumerable<KeyValuePair<string, object>> GetMetadata(Type targetType)
        {
            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }
            var propertyList = new List<KeyValuePair<string, object>>();

            foreach (var attribute in targetType.GetCustomAttributes(true)
                                                .Where(p => p.GetType().GetCustomAttributes(typeof(MetadataAttributeAttribute), false).Any()))
                propertyList.AddRange(GetProperties(attribute));

            return propertyList;
        }

        /// <summary>
        /// given a strong type, interrogate the attribution to retrieve an enumerable set of property names
        /// </summary>
        /// <typeparam name="TMetadataType">metadata type to look for in the list of attributes</typeparam>
        /// <param name="targetType">type to interrogate</param>
        /// <returns>enumerable set of properties found</returns>
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
