using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace AutofacContrib.Attributed
{
    public class MetadataHelper
    {
        private static IEnumerable<KeyValuePair<string, object>> GetProperties(object target)
        {
            return target.GetType()
                         .GetProperties()
                         .Where(propertyInfo => propertyInfo.CanRead &&
                                 propertyInfo.DeclaringType.Name != typeof(Attribute).Name)
                         .Select(propertyInfo =>
                                  new KeyValuePair<string, object>(propertyInfo.Name, propertyInfo.GetValue(target, null)));
        }


        public static IEnumerable<KeyValuePair<string, object>> GetMetadata(Type targetType)
        {
            var propertyList = new List<KeyValuePair<string, object>>();

            foreach (var attribute in targetType.GetCustomAttributes(true)
                                                .Where(p => p.GetType().GetCustomAttributes(typeof(MetadataAttributeAttribute), false).Count() > 0))
                propertyList.AddRange(GetProperties(attribute));

            return propertyList;
        }
    }
}
