using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Autofac.Builder;
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
        private static IDictionary<string, object> GetProperties(object attribute)
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

        /// <summary>
        /// This method can be invoked with the assembly scanner to register metadata that is declared loosely using
        /// attributes marked with the MetadataAttributeAttribute. All of the marked attributes are used together to create
        /// a common set of dictionary values that constitute the metadata on the type.
        /// </summary>
        /// <typeparam name="TLimit">The type of the limit</typeparam>
        /// <typeparam name="TScanningActivatorData">activator data type</typeparam>
        /// <typeparam name="TRegistrationStyle">registration style type</typeparam>
        /// <param name="builder">container builder</param>
        /// <returns>registration builder allowing the registration to be configured</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> 
            WithAttributedMetadata<TLimit, TScanningActivatorData, TRegistrationStyle>
                        (this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> builder)
                                        where TScanningActivatorData : ScanningActivatorData
        {
            // Count used otherwise the lazyness of the expression is one degree too lazy
            builder.ActivatorData.ConfigurationActions.Add(
                (t, rb) => GetMetadata(t).Select(rb.WithMetadata).Count());

            return builder;
        }


        /// <summary>
        /// This method can be invoked with the assembly scanner to register strongly typed metadata attributes. The
        /// attributes are scanned for one that is derived from the metadata interface.  If one is found, the metadata
        /// contents are extracted and registered with the instance registration
        /// </summary>
        /// <typeparam name="TMetadata">metadata type to search for</typeparam>
        /// <param name="builder">container builder</param>
        /// <returns>registration builder allowing the registration to be configured</returns>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            WithAttributedMetadata<TMetadata>(this IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> builder)
        {
            builder.ActivatorData.ConfigurationActions.Add(
                (t, rb) =>
                    {
                        var attribute =
                            (from p in t.GetCustomAttributes(typeof(TMetadata),true)
                             select p).FirstOrDefault();

                        if( attribute != null)
                            rb.WithMetadata(attribute.GetType().GetProperties().Where(propertyInfo => propertyInfo.CanRead)
                                                .Select(
                                                    propertyInfo =>
                                                    new KeyValuePair<string, object>(propertyInfo.Name,
                                                                                     propertyInfo.GetValue(attribute, null))));
                    });
            
            return builder;
        }
    }
}
