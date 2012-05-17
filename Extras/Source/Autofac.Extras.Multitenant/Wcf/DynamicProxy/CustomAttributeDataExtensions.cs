using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Autofac.Extras.Multitenant.Wcf.DynamicProxy
{
    /// <summary>
    /// Extension methods for <see cref="System.Reflection.CustomAttributeData"/>.
    /// </summary>
    public static class CustomAttributeDataExtensions
    {
        /// <summary>
        /// Converts a custom attribute data object to a custom attribute builder for code generation.
        /// </summary>
        /// <param name="data">The data about a custom attribute to be converted for code emission.</param>
        /// <returns>
        /// A <see cref="System.Reflection.Emit.CustomAttributeBuilder"/> with
        /// the same values as <paramref name="data" /> so it can be copied
        /// to another member in code generation.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="data" /> is <see langword="null" />.
        /// </exception>
        public static CustomAttributeBuilder ToAttributeBuilder(this CustomAttributeData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            var constructorArguments = new List<object>();
            foreach (var ctorArg in data.ConstructorArguments)
            {
                constructorArguments.Add(ctorArg.Value);
            }

            var propertyArguments = new List<PropertyInfo>();
            var propertyArgumentValues = new List<object>();
            var fieldArguments = new List<FieldInfo>();
            var fieldArgumentValues = new List<object>();
            foreach (var namedArg in data.NamedArguments)
            {
                var fi = namedArg.MemberInfo as FieldInfo;
                var pi = namedArg.MemberInfo as PropertyInfo;

                if (fi != null)
                {
                    fieldArguments.Add(fi);
                    fieldArgumentValues.Add(namedArg.TypedValue.Value);
                }
                else if (pi != null)
                {
                    propertyArguments.Add(pi);
                    propertyArgumentValues.Add(namedArg.TypedValue.Value);
                }
            }
            return new CustomAttributeBuilder(data.Constructor, constructorArguments.ToArray(), propertyArguments.ToArray(), propertyArgumentValues.ToArray(), fieldArguments.ToArray(), fieldArgumentValues.ToArray());
        }
    }
}
