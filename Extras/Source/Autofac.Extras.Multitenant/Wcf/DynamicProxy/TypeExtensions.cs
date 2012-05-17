using System;

namespace Autofac.Extras.Multitenant.Wcf.DynamicProxy
{
    /// <summary>
    /// Extension methods for the <see cref="System.Type"/> class.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Gets the metadata buddy class type, if any, as marked by a
        /// <see cref="Autofac.Extras.Multitenant.Wcf.ServiceMetadataTypeAttribute"/>
        /// </summary>
        /// <param name="interfaceType">The service interface type from which to retrieve the metadata class.</param>
        /// <returns>
        /// The metadata type for the service interface as specified by a
        /// <see cref="Autofac.Extras.Multitenant.Wcf.ServiceMetadataTypeAttribute"/>,
        /// if it exists; otherwise <see langword="null" />.
        /// </returns>
        public static Type GetMetadataClassType(this Type interfaceType)
        {
            if (interfaceType == null)
            {
                throw new ArgumentNullException("interfaceType");
            }
            var attribs = (ServiceMetadataTypeAttribute[])interfaceType.GetCustomAttributes(typeof(ServiceMetadataTypeAttribute), false);
            if (attribs.Length == 0)
            {
                return null;
            }
            return attribs[0].MetadataClassType;
        }
    }
}
