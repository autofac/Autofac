// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2012 Autofac Contributors
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Autofac.Core;

namespace Autofac.Features.Metadata
{
    internal static class MetadataViewProvider
    {
        private static readonly MethodInfo GetMetadataValueMethod = typeof(MetadataViewProvider).GetTypeInfo().GetDeclaredMethod("GetMetadataValue");

        public static Func<IDictionary<string, object>, TMetadata> GetMetadataViewProvider<TMetadata>()
        {
            if (typeof(TMetadata) == typeof(IDictionary<string, object>))
                return m => (TMetadata)m;

            if (!typeof(TMetadata).GetTypeInfo().IsClass)
            {
                throw new DependencyResolutionException(
                    string.Format(CultureInfo.CurrentCulture, MetadataViewProviderResources.InvalidViewImplementation, typeof(TMetadata).Name));
            }

            var ti = typeof(TMetadata);
            var dictionaryConstructor = ti.GetTypeInfo().DeclaredConstructors.SingleOrDefault(ci =>
            {
                var ps = ci.GetParameters();
                return ci.IsPublic && ps.Length == 1 && ps[0].ParameterType == typeof(IDictionary<string, object>);
            });

            if (dictionaryConstructor != null)
            {
                var providerArg = Expression.Parameter(typeof(IDictionary<string, object>), "metadata");
                return Expression.Lambda<Func<IDictionary<string, object>, TMetadata>>(
                        Expression.New(dictionaryConstructor, providerArg),
                        providerArg)
                    .Compile();
            }

            var parameterlessConstructor = ti.GetTypeInfo().DeclaredConstructors.SingleOrDefault(ci => ci.IsPublic && ci.GetParameters().Length == 0);
            if (parameterlessConstructor != null)
            {
                var providerArg = Expression.Parameter(typeof(IDictionary<string, object>), "metadata");
                var resultVar = Expression.Variable(typeof(TMetadata), "result");

                var resultAssignment = Expression.Assign(resultVar, Expression.New(parameterlessConstructor));
                var blockExprs = new List<Expression> { resultAssignment };

                foreach (var prop in typeof(TMetadata).GetRuntimeProperties()
                    .Where(prop =>
                        prop.GetMethod != null && !prop.GetMethod.IsStatic &&
                        prop.SetMethod != null && !prop.SetMethod.IsStatic))
                {
                    var dva = Expression.Constant(prop.GetCustomAttribute<DefaultValueAttribute>(false), typeof(DefaultValueAttribute));
                    var name = Expression.Constant(prop.Name, typeof(string));
                    var m = GetMetadataValueMethod.MakeGenericMethod(prop.PropertyType);
                    var assign = Expression.Assign(
                        Expression.Property(resultVar, prop),
                        Expression.Call(null, m, providerArg, name, dva));
                    blockExprs.Add(assign);
                }

                blockExprs.Add(resultVar);

                return Expression.Lambda<Func<IDictionary<string, object>, TMetadata>>(
                        Expression.Block(new[] { resultVar }, blockExprs), providerArg)
                    .Compile();
            }

            throw new DependencyResolutionException(
                string.Format(CultureInfo.CurrentCulture, MetadataViewProviderResources.InvalidViewImplementation, typeof(TMetadata).Name));
        }

        private static TValue GetMetadataValue<TValue>(IDictionary<string, object> metadata, string name, DefaultValueAttribute defaultValue)
        {
            object result;
            if (metadata.TryGetValue(name, out result))
                return (TValue)result;

            if (defaultValue != null)
                return (TValue)defaultValue.Value;

            throw new DependencyResolutionException(
                string.Format(CultureInfo.CurrentCulture, MetadataViewProviderResources.MissingMetadata, name));
        }
    }
}
