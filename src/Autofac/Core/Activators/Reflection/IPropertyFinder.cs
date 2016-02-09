using System;
using System.Reflection;

namespace Autofac.Core.Activators.Reflection
{
    /// <summary>
    /// Find suitable properties to inject
    /// </summary>
    public interface IPropertyFinder
    {
        /// <summary>
        /// Determine which properties on a type should be injected
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        PropertyInfo[] FindProperties(Type type);
    }
}
