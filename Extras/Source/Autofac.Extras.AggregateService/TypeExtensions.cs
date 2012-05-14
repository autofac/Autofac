using System;
using System.Collections.Generic;

namespace AutofacContrib.AggregateService
{
    internal static class TypeExtensions
    {
        /// <summary>
        /// Return unique interfaces implemented or inherited by <paramref name="type"/>.
        /// Will also include <paramref name="type"/> if it is an interface type.
        /// </summary>
        public static IEnumerable<Type> GetUniqueInterfaces(this Type type)
        {
            var types = new HashSet<Type>();
            foreach (var interfaceType in type.GetInterfaces())
            {
                if (types.Contains(interfaceType))
                    continue;
                types.Add(interfaceType);
            }
            
            if (type.IsInterface && !types.Contains(type))
            {
                types.Add(type);
            }

            return types;
        }
    }
}