// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace Autofac.Util
{
    /// <summary>
    /// Extension methods for reflection-related types.
    /// </summary>
    internal static class ReflectionExtensions
    {
        /// <summary>
        /// Create a typed delegate from a method info and the target object.
        /// </summary>
        /// <typeparam name="TDelegate">The delegate.</typeparam>
        /// <param name="method">The method.</param>
        /// <param name="target">The target object for the delegate.</param>
        /// <returns>A constructed delegate.</returns>
        public static TDelegate CreateDelegate<TDelegate>(this MethodInfo method, object? target)
            where TDelegate : Delegate
            => (TDelegate)method.CreateDelegate(typeof(TDelegate), target);

        /// <summary>
        /// Maps from a property-set-value parameter to the declaring property.
        /// </summary>
        /// <param name="pi">Parameter to the property setter.</param>
        /// <param name="prop">The property info on which the setter is specified.</param>
        /// <returns>True if the parameter is a property setter.</returns>
        public static bool TryGetDeclaringProperty(this ParameterInfo pi, [NotNullWhen(returnValue: true)] out PropertyInfo? prop)
        {
            var mi = pi.Member as MethodInfo;
            if (mi != null && mi.IsSpecialName && mi.Name.StartsWith("set_", StringComparison.Ordinal) && mi.DeclaringType != null)
            {
                prop = mi.DeclaringType.GetDeclaredProperty(mi.Name.Substring(4));
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
            if (propertyAccessor == null)
            {
                throw new ArgumentNullException(nameof(propertyAccessor));
            }

            var mex = propertyAccessor.Body as MemberExpression;
            if (!(mex?.Member is PropertyInfo))
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture,
                    ReflectionExtensionsResources.ExpressionNotPropertyAccessor,
                    propertyAccessor));
            }

            return (PropertyInfo)mex.Member;
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
            if (methodCallExpression == null)
            {
                throw new ArgumentNullException(nameof(methodCallExpression));
            }

            if (!(methodCallExpression.Body is MethodCallExpression callExpression))
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture,
                    ReflectionExtensionsResources.ExpressionNotMethodCall,
                    methodCallExpression));
            }

            return callExpression.Method;
        }

        /// <summary>
        /// Gets the <see cref="ConstructorInfo"/> for the new operation called in the expression.
        /// </summary>
        /// <typeparam name="TDeclaring">The type on which the constructor is called.</typeparam>
        /// <param name="constructorCallExpression">Expression demonstrating how the constructor is called.</param>
        /// <returns>The <see cref="ConstructorInfo"/> for the called constructor.</returns>
        public static ConstructorInfo GetConstructor<TDeclaring>(
            Expression<Func<TDeclaring>> constructorCallExpression)
        {
            if (constructorCallExpression == null)
            {
                throw new ArgumentNullException(nameof(constructorCallExpression));
            }

            if (!(constructorCallExpression.Body is NewExpression callExpression))
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture,
                    ReflectionExtensionsResources.ExpressionNotConstructorCall,
                    constructorCallExpression));
            }

            return callExpression.Constructor!;
        }
    }
}
