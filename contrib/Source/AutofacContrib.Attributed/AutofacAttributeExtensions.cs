using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Builder;
using Autofac.Features.Metadata;
using Autofac.Features.Scanning;

namespace AutofacContrib.Attributed
{
    public static class AutofacAttributeExtensions
    {
        #region discovery suppport methods

        /// <summary>
        /// retrieves a dictionary of public properties on an attribute
        /// </summary>
        /// <param name="attribute">attribute being queried</param>
        /// <returns>dictionary of property names and their values</returns>
        private static IDictionary<string, object> GetProperties(Attribute attribute)
        {
            return attribute.GetType().GetProperties().Where(propertyInfo => propertyInfo.CanRead &&
                                                                      propertyInfo.DeclaringType.Name !=
                                                                      typeof(Attribute).Name)
                .Select(propertyInfo => new KeyValuePair<string, object>
                                            (propertyInfo.Name, propertyInfo.GetValue(attribute, null))).ToDictionary(pair => pair.Key, pair => pair.Value);


        }

        private static IEnumerable<IDictionary<string, object>> GetMetadata(Type targetType)
        {
            return from Attribute attribute in targetType.GetCustomAttributes(true)
                   where attribute.GetType().GetCustomAttributes(typeof(MetadataAttributeAttribute), false).Count() > 0
                   select GetProperties(attribute);
        }


        #endregion


        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> 
            WithAttributedMetadata<TLimit, TScanningActivatorData, TRegistrationStyle>
                        (this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration)
                                        where TScanningActivatorData : ScanningActivatorData
        {
            // Count required otherwise the lazyness of the expression is one degree too lazy
            registration.ActivatorData.ConfigurationActions.Add(
                
                (t, rb) => GetMetadata(t).Select(rb.WithMetadata).Count());

            return registration;
        }



    }
}
