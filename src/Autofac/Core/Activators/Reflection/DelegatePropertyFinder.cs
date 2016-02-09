using System;
using System.Reflection;

namespace Autofac.Core.Activators.Reflection
{
    public class DelegatePropertyFinder : IPropertyFinder
    {
        private readonly Func<Type, PropertyInfo[]> _finder;

        public DelegatePropertyFinder(Func<Type, PropertyInfo[]> finder)
        {
            if (finder == null) throw new ArgumentNullException(nameof(finder));

            _finder = finder; 
        }

        public PropertyInfo[] FindProperties(Type type)
        {
            return _finder(type);
        }
    }
}
