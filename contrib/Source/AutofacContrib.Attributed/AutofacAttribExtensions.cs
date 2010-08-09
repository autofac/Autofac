using System;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Features.Metadata;
using AutofacContrib.Attributed.MEF;

namespace AutofacContrib.Attributed
{
    public static class AutofacAttribExtensions
    {
        public static void RegisterUsingMetadataAttributes<TInterface, TMetadata>(this ContainerBuilder builder, params Assembly[] assemblies)
        {
            builder.RegisterUsingMetadataAttributes<TInterface, TMetadata>(p => true, assemblies);
        }

        public static void RegisterUsingMetadataAttributes<TInterface, TMetadata>(this ContainerBuilder builder, Predicate<TMetadata> inclusionPredicate, params Assembly[] assemblies)
        {
            if(inclusionPredicate == null)
                throw new ArgumentNullException("inclusionPredicate");

            var aggregateCatalog = new AggregateCatalog(from a in assemblies select new AssemblyCatalog(a));

            var container = new ExportInfoCompositionContainer(aggregateCatalog);

            var exports = container.GetExportsWithTargetType<TInterface, TMetadata>();
            
            foreach (var export in exports.Where(a => inclusionPredicate(a.Value.Metadata)))
            {
                // first, register the target type in the container
                builder.RegisterType(export.InstantiationType);

                // next, create a Lazy<t,meta> registration wired up to the strongly typed metadata
                // make sure we hold a local variable for proper closure behavior
                var item = export;
                builder.Register(
                    c => new Lazy<TInterface, TMetadata>(() => (TInterface)c.Resolve(item.InstantiationType),
                                                         item.Value.Metadata));

                // do we really need to do this separately from the Lazy<T, TM> wireup?
                builder.Register(
                    c => new Meta<TInterface, TMetadata>((TInterface)c.Resolve(item.InstantiationType), item.Value.Metadata));

            }
        }
    }
}
