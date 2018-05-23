using System;
using System.Reflection;

namespace Autofac.Core
{
    /// <summary>
    /// Provides a property selector that applies a filter defined by a delegate.
    /// </summary>
    public sealed class DelegatePropertySelector : IPropertySelector
    {
        private readonly Func<PropertyInfo, object, bool> _finder;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegatePropertySelector"/> class
        /// that invokes a delegate to determine selection.
        /// </summary>
        /// <param name="finder">Delegate to determine whether a property should be injected.</param>
        public DelegatePropertySelector(Func<PropertyInfo, object, bool> finder)
        {
            if (finder == null) throw new ArgumentNullException(nameof(finder));

            _finder = finder;
        }

        public bool InjectProperty(PropertyInfo property, object instance)
        {
            return _finder(property, instance);
        }
    }
}
