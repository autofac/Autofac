using System;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using Autofac;
using AutofacContrib.Attributed.MEF;

namespace AutofacContrib.Attributed
{
    public static class AutofacAttribExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <typeparam name="TMetadata"></typeparam>
        /// <param name="builder"></param>
        /// <param name="assemblies"></param>
        public static void RegisterUsingMetadataAttributes<TInterface, TMetadata>(this ContainerBuilder builder, params Assembly[] assemblies)
        {
            var aggregateCatalog = new AggregateCatalog(from a in assemblies select new AssemblyCatalog(a));

            var container = new ExportInfoCompositionContainer(aggregateCatalog);

            var exports = container.GetExportsWithTargetType<TInterface, TMetadata>();


            foreach (var export in exports)
            {
                // first, register the target type in the container
                builder.RegisterType(export.InstantiationType);

                // next, create a Lazy<t,meta> registration wired up to the strongly typed metadata
                // make sure we hold a local variable for proper closure behavior
                var item = export;
                builder.Register(
                    c => new Lazy<TInterface, TMetadata>(() => (TInterface)c.Resolve(item.InstantiationType),
                                                         item.Value.Metadata));

            }
        }

    }
}
