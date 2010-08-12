using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Features.Metadata;

namespace AutofacContrib.Attributed
{
    public static class AutofacAttribExtensions
    {
        public static void RegisterUsingMetadataAttributes<TInterface, TMetadata>(this ContainerBuilder builder, params Assembly[] assemblies)
        {
            builder.RegisterUsingMetadataAttributes<TInterface, TMetadata>(p => true, assemblies);
        }


        private static bool HasMetadataAttribute(Type typeInfo)
        {
            return typeInfo.GetCustomAttributes(true).Cast<Attribute>().Any(info => info.GetType().GetCustomAttributes(typeof(MetadataAttributeAttribute), false).Count() > 0);
        }

        private static IDictionary<string,object> GetProperties(Attribute attribute)
        {
            return attribute.GetType().GetProperties().Where(propertyInfo => propertyInfo.CanRead &&
                                                                      propertyInfo.DeclaringType.Name !=
                                                                      typeof (Attribute).Name)
                .Select(propertyInfo => new KeyValuePair<string, object>
                                            (propertyInfo.Name, propertyInfo.GetValue(attribute, null))).ToDictionary(pair => pair.Key, pair => pair.Value);


        }

        private static IEnumerable<TMetadata> GetStronglyTypedMetadata<TMetadata>(Type targetType)
        {
            return from Attribute attribute in targetType.GetCustomAttributes(true) where attribute.GetType().GetCustomAttributes(typeof(MetadataAttributeAttribute), false).Count() >0 select AttributedModelServices.GetMetadataView<TMetadata>(GetProperties(attribute));
        }

        public static void RegisterUsingMetadataAttributes<TInterface, TMetadata>(this ContainerBuilder builder, Predicate<TMetadata> inclusionPredicate, params Assembly[] assemblies)
        {
            if (inclusionPredicate == null)
                throw new ArgumentNullException("inclusionPredicate");
            foreach (var targetType in assemblies.Select(assembly => (from type in assembly.GetTypes()
                                                                      where type.IsClass && type.GetInterface(typeof (TInterface).Name) != null && HasMetadataAttribute(type)
                                                                      select type)).SelectMany(targetTypes => targetTypes))
            {
                builder.RegisterType(targetType);

                // get all TMetadata instances

                foreach (var metadata in GetStronglyTypedMetadata<TMetadata>(targetType).Where(a => inclusionPredicate(a)))
                {
                    // hold onto the ambient properties in local scope declarations to service the lambda-lifted closure
                    var localType = targetType;
                    var localMetadata = metadata;
                    builder.Register(
                        c => new Lazy<TInterface, TMetadata>(() => (TInterface) c.Resolve(localType), localMetadata));

                    // support the autofac strongly typed Meta wireup
                    builder.Register(
                        c => new Meta<TInterface, TMetadata>((TInterface) c.Resolve(localType), localMetadata));
                }
            }
        }
    }
}
