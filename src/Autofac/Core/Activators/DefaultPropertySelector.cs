using System;
using System.Reflection;

namespace Autofac.Core
{
    /// <summary>
    /// Provides default property selector that applies appropriate filters to ensure only
    /// public settable properties are selected (including filtering for value types and indexed
    /// properties).
    /// </summary>
    public class DefaultPropertySelector : IPropertySelector
    {
        /// <summary>
        /// Gets an instance of DefaultPropertySelector that will preserve any values already set
        /// </summary>
        internal static IPropertySelector PreserveSetValueInstance { get; } = new DefaultPropertySelector(true);

        /// <summary>
        /// Gets an instance of DefaultPropertySelector that will cause values to be overwritten
        /// </summary>
        internal static IPropertySelector OverwriteSetValueInstance { get; } = new DefaultPropertySelector(false);

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultPropertySelector"/> class
        /// that provides default selection criteria.
        /// </summary>
        /// <param name="preserveSetValues">Determines if values should be preserved or not</param>
        public DefaultPropertySelector(bool preserveSetValues)
        {
            PreserveSetValues = preserveSetValues;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the value should be set if the value is already
        /// set (ie non-null)
        /// </summary>
        public bool PreserveSetValues { get; protected set; }

        /// <summary>
        /// Provides default filtering to determine if property should be injected by rejecting
        /// non-public settable properties.
        /// </summary>
        /// <param name="propertyInfo">Property to be injected</param>
        /// <param name="instance">Instance that has the property to be injected</param>
        /// <returns>Whether property should be injected</returns>
        public virtual bool InjectProperty(PropertyInfo propertyInfo, object instance)
        {
            if (propertyInfo.SetMethod?.IsPublic != true)
                return false;

            if (PreserveSetValues && propertyInfo.CanRead && propertyInfo.CanWrite &&
                (propertyInfo.GetValue(instance, null) != null))
                return false;

            return true;
        }
    }
}
