// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Autofac.Core;

namespace Autofac.Features.Metadata;

/// <summary>
/// Helper methods for creating a metadata access function that retrieves typed metadata from a dictionary.
/// </summary>
internal static class MetadataViewProvider
{
    private static readonly MethodInfo _getMetadataValueMethod = typeof(MetadataViewProvider).GetDeclaredMethod(nameof(GetMetadataValue));

    /// <summary>
    /// Generate a provider function that takes a dictionary of metadata, and outputs a typed metadata object.
    /// </summary>
    /// <remarks>
    /// The view factory is compiled once per metadata type. Each lifetime scope that adapts a
    /// registration to <c>Meta&lt;T, TMetadata&gt;</c> or <c>Lazy&lt;T, TMetadata&gt;</c> asks for a provider again.
    /// Each provider reads its own <see cref="DefaultValueAttribute"/> instances, so a mutable default
    /// is shared only by the views of one registration.
    /// </remarks>
    /// <typeparam name="TMetadata">The metadata type.</typeparam>
    /// <returns>A provider function.</returns>
    public static Func<IDictionary<string, object?>, TMetadata> GetMetadataViewProvider<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)] TMetadata>()
    {
        var (properties, factory) = ReflectionCacheSet.Shared.Internal.MetadataViewFactories.GetOrAdd(
            typeof(TMetadata),
            static _ => CreateMetadataViewFactory<TMetadata>());

        var create = (Func<IDictionary<string, object?>, DefaultValueAttribute?[], TMetadata>)factory;
        var defaults = new DefaultValueAttribute?[properties.Length];
        for (var i = 0; i < properties.Length; i++)
        {
            defaults[i] = properties[i].GetCustomAttribute<DefaultValueAttribute>(false);
        }

        return metadata => create(metadata, defaults);
    }

    [UnconditionalSuppressMessage(
        "AOT",
        "IL3050:RequiresDynamicCode",
        Justification = "Strongly-typed metadata views compile an expression tree and use MakeGenericMethod over each property type. This is only reached when a consumer resolves a Meta<T, TMetadata> / Lazy<T, TMetadata> relationship (an always-registered implicit source dispatched by relationship type, which cannot surface the requirement at the consumer's call site). Consumers that resolve strongly-typed metadata views take on the dynamic-code requirement.")]
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2060:MakeGenericMethod",
        Justification = "The generic argument is the metadata view property's own type. The properties of TMetadata are preserved by this method's [DynamicallyAccessedMembers(PublicProperties)] annotation, so the property types are reachable.")]
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2087:UnrecognizedReflectionPattern",
        Justification = "GetRuntimeProperties also enumerates non-public properties, but only public settable properties of the metadata view are populated (and those are preserved by the PublicProperties annotation). Non-public properties are not part of the supported metadata-view surface.")]
    private static (PropertyInfo[] Properties, Delegate Factory) CreateMetadataViewFactory<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)] TMetadata>()
    {
        if (typeof(TMetadata) == typeof(IDictionary<string, object>))
        {
            return (Array.Empty<PropertyInfo>(), new Func<IDictionary<string, object?>, DefaultValueAttribute?[], TMetadata>((m, _) => (TMetadata)m));
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

        var defaultsArg = Expression.Parameter(typeof(DefaultValueAttribute?[]), "defaults");

        if (dictionaryConstructor is not null)
        {
            var providerArg = Expression.Parameter(typeof(IDictionary<string, object?>), "metadata");
            var factory = Expression.Lambda<Func<IDictionary<string, object?>, DefaultValueAttribute?[], TMetadata>>(
                    Expression.New(dictionaryConstructor, providerArg),
                    providerArg,
                    defaultsArg)
                .Compile();
            return (Array.Empty<PropertyInfo>(), factory);
        }

        var parameterlessConstructor = publicConstructors.SingleOrDefault(ci => ci.GetParameters().Length == 0);
        if (parameterlessConstructor is not null)
        {
            var providerArg = Expression.Parameter(typeof(IDictionary<string, object>), "metadata");
            var resultVar = Expression.Variable(typeof(TMetadata), "result");

            var resultAssignment = Expression.Assign(resultVar, Expression.New(parameterlessConstructor));
            var blockExprs = new List<Expression> { resultAssignment };

            var properties = typeof(TMetadata).GetRuntimeProperties()
                .Where(prop =>
                    prop.GetMethod is not null && !prop.GetMethod.IsStatic &&
                    prop.SetMethod is not null && !prop.SetMethod.IsStatic)
                .ToArray();

            for (var i = 0; i < properties.Length; i++)
            {
                var prop = properties[i];
                var dva = Expression.ArrayIndex(defaultsArg, Expression.Constant(i));
                var name = Expression.Constant(prop.Name, typeof(string));
                var m = _getMetadataValueMethod.MakeGenericMethod(prop.PropertyType);
                var assign = Expression.Assign(
                    Expression.Property(resultVar, prop),
                    Expression.Call(null, m, providerArg, name, dva));
                blockExprs.Add(assign);
            }

            blockExprs.Add(resultVar);

            var factory = Expression.Lambda<Func<IDictionary<string, object?>, DefaultValueAttribute?[], TMetadata>>(
                    Expression.Block(new[] { resultVar }, blockExprs), providerArg, defaultsArg)
                .Compile();
            return (properties, factory);
        }

        throw new DependencyResolutionException(
            string.Format(CultureInfo.CurrentCulture, MetadataViewProviderResources.InvalidViewImplementation, typeof(TMetadata).Name));
    }

    private static TValue? GetMetadataValue<TValue>(IDictionary<string, object> metadata, string name, DefaultValueAttribute defaultValue)
    {
        if (metadata.TryGetValue(name, out var result))
        {
            return (TValue)result;
        }

        if (defaultValue is not null)
        {
            return (TValue?)defaultValue.Value;
        }

        throw new DependencyResolutionException(
            string.Format(CultureInfo.CurrentCulture, MetadataViewProviderResources.MissingMetadata, name));
    }
}
