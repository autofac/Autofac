using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Autofac.Util
{
    /// <summary>
    /// Extension methods for reflection-related types.
    /// </summary>
    static class ReflectionExtensions
    {
        /// <summary>
        /// Maps from a property-set-value parameter to the declaring property.
        /// </summary>
        /// <param name="pi">Parameter to the property setter.</param>
        /// <param name="prop">The property info on which the setter is specified.</param>
        /// <returns>True if the parameter is a property setter.</returns>
        public static bool TryGetDeclaringProperty(this ParameterInfo pi, out PropertyInfo prop)
        {
            var mi = pi.Member as MethodInfo;
            if (mi != null && mi.IsSpecialName && mi.Name.StartsWith("set_"))
            {
                prop = mi.DeclaringType.GetProperty(mi.Name.Substring(4));
                return true;
            }

            prop = null;
            return false;
        }

        /// <summary>
        /// Get a PropertyInfo object from an expression of the form
        /// x =&gt; x.P.
        /// </summary>
        /// <typeparam name="TDeclaring">Type declaring the property.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="propertyAccessor">Expression mapping an instance of the
        /// declaring type to the property value.</param>
        /// <returns>Property info.</returns>
        public static PropertyInfo GetProperty<TDeclaring, TProperty>(
            Expression<Func<TDeclaring, TProperty>> propertyAccessor)
        {
            Enforce.ArgumentNotNull(propertyAccessor, "propertyAccessor");
            var mex = propertyAccessor.Body as MemberExpression;
            if (mex == null ||
                mex.Member.MemberType != MemberTypes.Property)
                throw new ArgumentException(string.Format(
                    ReflectionExtensionsResources.ExpressionNotPropertyAccessor,
                    propertyAccessor));
            return (PropertyInfo) mex.Member;
        }

        public static bool IsFunction(this Type type)
        {
            Enforce.ArgumentNotNull(type, "type");
            return type.IsSubclassOf(typeof(Delegate));
        }

        public static Type FunctionReturnType(this Type type)
        {
            Enforce.ArgumentNotNull(type, "type");
            var invoke = type.GetMethod("Invoke");
            Enforce.NotNull(invoke);
            return invoke.ReturnType;
        }

        public static bool IsClosingTypeOf(this Type type, Type openGenericType)
        {
            Enforce.ArgumentNotNull(type, "type");
            Enforce.ArgumentNotNull(openGenericType, "openGenericType");
            return type.IsGenericType && type.GetGenericTypeDefinition() == openGenericType;
        }
    }
}
