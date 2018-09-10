using System.Reflection;

namespace Autofac.Core
{
    /// <summary>
    /// Finds suitable properties to inject.
    /// </summary>
    public interface IPropertySelector
    {
        /// <summary>
        /// Provides filtering to determine if property should be injected.
        /// </summary>
        /// <param name="propertyInfo">Property to be injected.</param>
        /// <param name="instance">Instance that has the property to be injected.</param>
        /// <returns>Whether property should be injected.</returns>
        bool InjectProperty(PropertyInfo propertyInfo, object instance);
    }
}
