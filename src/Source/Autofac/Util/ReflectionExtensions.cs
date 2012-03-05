// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

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
            if (propertyAccessor == null) throw new ArgumentNullException("propertyAccessor");
            var mex = propertyAccessor.Body as MemberExpression;
            if (mex == null ||
                !(mex.Member is PropertyInfo))
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
            if (methodCallExpression == null) throw new ArgumentNullException("methodCallExpression");
            var callExpression = methodCallExpression.Body as MethodCallExpression;
            if (callExpression == null)
                throw new ArgumentException(string.Format(
                    ReflectionExtensionsResources.ExpressionNotMethodCall,
                    methodCallExpression));
            return callExpression.Method;
        }
    }
}
