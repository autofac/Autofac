using System;
using System.Reflection;

namespace Autofac.Core.Activators.Reflection
{
    public class DefaultPropertyFinder : IPropertyFinder
    {
        private static readonly PropertyInfo[] s_empty = new PropertyInfo[] { };

        public PropertyInfo[] FindProperties(Type type)
        {
            return s_empty;
        }
    }
}
