using System;
using System.Linq;
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

        /// <summary>
        /// Get the MethodInfo for a method called in the
        /// expression.
        /// </summary>
        /// <typeparam name="TDeclaring">Type on which the method is called.</typeparam>
        /// <param name="methodCallExpression">Expression demonstrating how the method appears.</param>
        /// <returns>The method info for the called method.</returns>
        public static MethodInfo GetMethod<TDeclaring>(
            Expression<Action<TDeclaring>> methodCallExpression)
        {
            Enforce.ArgumentNotNull(methodCallExpression, "methodCallExpression");
            var callExpression = methodCallExpression.Body as MethodCallExpression;
            if (callExpression == null)
                throw new ArgumentException(string.Format(
                    ReflectionExtensionsResources.ExpressionNotMethodCall,
                    methodCallExpression));
            return callExpression.Method;
        }

        public static bool IsDelegate(this Type type)
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

        public static bool IsCompatibleWithGenericParameters(this Type genericTypeDefinition, Type[] parameters)
        {
            var genericArgumentDefinitions = genericTypeDefinition.GetGenericArguments();

            for (var i = 0; i < genericArgumentDefinitions.Length; ++i)
            {
                var argumentDefinition = genericArgumentDefinitions[i];
                var parameter = parameters[i];

                if (argumentDefinition.GetGenericParameterConstraints()
                    .Any(constraint => !ParameterCompatibleWithTypeConstraint(parameter, constraint)))
                {
                    return false;
                }

                var specialConstraints = argumentDefinition.GenericParameterAttributes;

                if ((specialConstraints & GenericParameterAttributes.DefaultConstructorConstraint)
                    != GenericParameterAttributes.None)
                {
                    if (!parameter.IsValueType && parameter.GetConstructor(Type.EmptyTypes) == null)
                        return false;
                }

                if ((specialConstraints & GenericParameterAttributes.ReferenceTypeConstraint)
                    != GenericParameterAttributes.None)
                {
                    if (parameter.IsValueType)
                        return false;
                }

                if ((specialConstraints & GenericParameterAttributes.NotNullableValueTypeConstraint)
                    != GenericParameterAttributes.None)
                {
                    if (!parameter.IsValueType ||
                        (parameter.IsGenericType && parameter.IsClosingTypeOf(typeof(Nullable<>))))
                        return false;
                }
            }

            return true;
        }

        static bool ParameterCompatibleWithTypeConstraint(Type parameter, Type constraint)
        {
            return constraint.IsAssignableFrom(parameter) ||
                (parameter.IsClass &&
                   Traverse.Across(parameter, p => p.BaseType)
                       .Concat(parameter.GetInterfaces())
                       .Any(p => p.GUID == constraint.GUID));
        }

        public static bool IsCompatibleWith(this Type type, Type that)
        {

#if !(SILVERLIGHT || NET35)
            return type.IsEquivalentTo(that);
#else
            return type.Equals(that);
#endif
        }

        public static int GetCompatibleHashCode(this Type type)
        {
            if (type.IsImport)
                return type.GUID.GetHashCode();

            return type.GetHashCode();
        }
    }
}
