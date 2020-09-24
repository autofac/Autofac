// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
    /// <summary>
    /// Helper methods for creating a metadata access function that retrieves typed metdata from a dictionary.
    /// </summary>
    internal static class MetadataViewProvider
    {
        private static readonly MethodInfo GetMetadataValueMethod = typeof(MetadataViewProvider).GetDeclaredMethod(nameof(GetMetadataValue));

        /// <summary>
        /// Generate a provider function that takes a dictionary of metadata, and outputs a typed metadata object.
        /// </summary>
        /// <typeparam name="TMetadata">The metadata type.</typeparam>
        /// <returns>A provider function.</returns>
        public static Func<IDictionary<string, object?>, TMetadata> GetMetadataViewProvider<TMetadata>()
        {
            if (typeof(TMetadata) == typeof(IDictionary<string, object>))
            {
                return m => (TMetadata)m;
            }

            if (!typeof(TMetadata).IsClass)
            {
                throw new DependencyResolutionException(
                    string.Format(CultureInfo.CurrentCulture, MetadataViewProviderResources.InvalidViewImplementation, typeof(TMetadata).Name));
            }

            var ti = typeof(TMetadata);
            var publicConstructors = ti.GetDeclaredPublicConstructors();

            var dictionaryConstructor = publicConstructors.SingleOrDefault(ci =>
            {
                var ps = ci.GetParameters();
                return ps.Length == 1 && ps[0].ParameterType == typeof(IDictionary<string, object>);
            });

            if (dictionaryConstructor != null)
            {
                var providerArg = Expression.Parameter(typeof(IDictionary<string, object?>), "metadata");
                return Expression.Lambda<Func<IDictionary<string, object?>, TMetadata>>(
                        Expression.New(dictionaryConstructor, providerArg),
                        providerArg)
                    .Compile();
            }

            var parameterlessConstructor = publicConstructors.SingleOrDefault(ci => ci.GetParameters().Length == 0);
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

                return Expression.Lambda<Func<IDictionary<string, object?>, TMetadata>>(
                        Expression.Block(new[] { resultVar }, blockExprs), providerArg)
                    .Compile();
            }

            throw new DependencyResolutionException(
                string.Format(CultureInfo.CurrentCulture, MetadataViewProviderResources.InvalidViewImplementation, typeof(TMetadata).Name));
        }

        private static TValue GetMetadataValue<TValue>(IDictionary<string, object> metadata, string name, DefaultValueAttribute defaultValue)
        {
            if (metadata.TryGetValue(name, out object result))
            {
                return (TValue)result;
            }

            if (defaultValue != null)
            {
                return (TValue)defaultValue.Value;
            }

            throw new DependencyResolutionException(
                string.Format(CultureInfo.CurrentCulture, MetadataViewProviderResources.MissingMetadata, name));
        }
    }
}
