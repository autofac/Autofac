// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2008 Autofac Contributors
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
using System.Linq;
using System.ComponentModel.Composition;
using System.Reflection;
using Autofac.Builder;
using Autofac.Registrars;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.Hosting;

namespace Autofac.Integration.Mef
{
    /// <summary>
    /// Extension methods that add MEF hosting capabilities to the container building classes.
    /// </summary>
    public static class MefExtensions
    {
        static IDictionary<string, object> EmptyMetadata = new Dictionary<string, object>();
            
        /// <summary>
        /// Expose the registered service to MEF parts as an export.
        /// </summary>
        /// <param name="registrar">The component being registered.</param>
        /// <param name="contractName">The contract name that appears in the MEF ExportDefinition.</param>
        /// <returns>A registrar allowing registration to continue.</returns>
        public static IConcreteRegistrar ExportedAs(this IConcreteRegistrar registrar, string contractName)
        {
            return ExportedAs(registrar, contractName, EmptyMetadata);
        }

        /// <summary>
        /// Expose the registered service to MEF parts as an export.
        /// </summary>
        /// <param name="registrar">The component being registered.</param>
        /// <param name="contractName">The contract name that appears in the MEF ExportDefinition.</param>
        /// <param name="metadata">The metadata values associated with the MEF ExportDefinition.</param>
        /// <returns>A registrar allowing registration to continue.</returns>
        public static IConcreteRegistrar ExportedAs(this IConcreteRegistrar registrar, string contractName, IDictionary<string, object> metadata)
        {
            if (registrar == null) throw new ArgumentNullException("registrar");
            if (contractName == null) throw new ArgumentNullException("contractName");
            if (metadata == null) throw new ArgumentNullException("metadata");

            registrar.OnRegistered((s, e) => AttachExport(e.Container, e.Registration, contractName, metadata));

            return registrar;
        }

        /// <summary>
        /// Expose the registered service to MEF parts as an export.
        /// </summary>
        /// <param name="registrar">The component being registered.</param>
        /// <param name="contractType">The contract type that appears in the MEF ExportDefinition.</param>
        /// <returns>A registrar allowing registration to continue.</returns>
        public static IConcreteRegistrar ExportedAs(this IConcreteRegistrar registrar, Type contractType)
        {
            return ExportedAs(registrar, contractType, EmptyMetadata);
        }

        /// <summary>
        /// Expose the registered service to MEF parts as an export.
        /// </summary>
        /// <param name="registrar">The component being registered.</param>
        /// <typeparam name="ContractType">The contract type that appears in the MEF ExportDefinition.</typeparam>
        /// <returns>A registrar allowing registration to continue.</returns>
        public static IConcreteRegistrar ExportedAs<ContractType>(this IConcreteRegistrar registrar)
        {
            return ExportedAs(registrar, typeof(ContractType), EmptyMetadata);
        }

        /// <summary>
        /// Expose the registered service to MEF parts as an export.
        /// </summary>
        /// <param name="registrar">The component being registered.</param>
        /// <param name="contractType">The contract type that appears in the MEF ExportDefinition.</param>
        /// <param name="metadata">The metadata values associated with the MEF ExportDefinition.</param>
        /// <returns>A registrar allowing registration to continue.</returns>
        public static IConcreteRegistrar ExportedAs(this IConcreteRegistrar registrar, Type contractType, IDictionary<string, object> metadata)
        {
            return ExportedAs(registrar, CompositionServices.GetContractName(contractType), metadata);
        }

        /// <summary>
        /// Expose the registered service to MEF parts as an export.
        /// </summary>
        /// <param name="registrar">The component being registered.</param>
        /// <typeparam name="ContractType">The contract type that appears in the MEF ExportDefinition.</typeparam>
        /// <param name="metadata">The metadata values associated with the MEF ExportDefinition.</param>
        /// <returns>A registrar allowing registration to continue.</returns>
        public static IConcreteRegistrar ExportedAs<ContractType>(this IConcreteRegistrar registrar, IDictionary<string, object> metadata)
        {
            return ExportedAs(registrar, typeof(ContractType), metadata);
        }

        /// <summary>
        /// Expose the registered service to MEF parts as an export.
        /// </summary>
        /// <param name="registrar">The component being registered.</param>
        /// <param name="contractName">The contract name that appears in the MEF ExportDefinition.</param>
        /// <returns>A registrar allowing registration to continue.</returns>
        public static IReflectiveRegistrar ExportedAs(this IReflectiveRegistrar registrar, string contractName)
        {
            return ExportedAs(registrar, contractName, EmptyMetadata);
        }

        /// <summary>
        /// Expose the registered service to MEF parts as an export.
        /// </summary>
        /// <param name="registrar">The component being registered.</param>
        /// <param name="contractName">The contract name that appears in the MEF ExportDefinition.</param>
        /// <param name="metadata">The metadata values associated with the MEF ExportDefinition.</param>
        /// <returns>A registrar allowing registration to continue.</returns>
        public static IReflectiveRegistrar ExportedAs(this IReflectiveRegistrar registrar, string contractName, IDictionary<string, object> metadata)
        {
            if (registrar == null) throw new ArgumentNullException("registrar");
            if (contractName == null) throw new ArgumentNullException("contractName");
            if (metadata == null) throw new ArgumentNullException("metadata");

            registrar.OnRegistered((s, e) => AttachExport(e.Container, e.Registration, contractName, metadata));

            return registrar;
        }

        /// <summary>
        /// Expose the registered service to MEF parts as an export.
        /// </summary>
        /// <param name="registrar">The component being registered.</param>
        /// <param name="contractType">The contract type that appears in the MEF ExportDefinition.</param>
        /// <returns>A registrar allowing registration to continue.</returns>
        public static IReflectiveRegistrar ExportedAs(this IReflectiveRegistrar registrar, Type contractType)
        {
            return ExportedAs(registrar, contractType, EmptyMetadata);
        }

        /// <summary>
        /// Expose the registered service to MEF parts as an export.
        /// </summary>
        /// <param name="registrar">The component being registered.</param>
        /// <typeparam name="ContractType">The contract type that appears in the MEF ExportDefinition.</typeparam>
        /// <param name="metadata">The metadata values associated with the MEF ExportDefinition.</param>
        /// <returns>A registrar allowing registration to continue.</returns>
        public static IReflectiveRegistrar ExportedAs<ContractType>(this IReflectiveRegistrar registrar)
        {
            return ExportedAs(registrar, typeof(ContractType), EmptyMetadata);
        }

        /// <summary>
        /// Expose the registered service to MEF parts as an export.
        /// </summary>
        /// <param name="registrar">The component being registered.</param>
        /// <param name="contractType">The contract type that appears in the MEF ExportDefinition.</param>
        /// <param name="metadata">The metadata values associated with the MEF ExportDefinition.</param>
        /// <returns>A registrar allowing registration to continue.</returns>
        public static IReflectiveRegistrar ExportedAs(this IReflectiveRegistrar registrar, Type contractType, IDictionary<string, object> metadata)
        {
            return ExportedAs(registrar, CompositionServices.GetContractName(contractType), metadata);
        }

        /// <summary>
        /// Expose the registered service to MEF parts as an export.
        /// </summary>
        /// <param name="registrar">The component being registered.</param>
        /// <typeparam name="ContractType">The contract type that appears in the MEF ExportDefinition.</typeparam>
        /// <param name="metadata">The metadata values associated with the MEF ExportDefinition.</param>
        /// <returns>A registrar allowing registration to continue.</returns>
        public static IReflectiveRegistrar ExportedAs<ContractType>(this IReflectiveRegistrar registrar, IDictionary<string, object> metadata)
        {
            return ExportedAs(registrar, typeof(ContractType), metadata);
        }

        /// <summary>
        /// Register a MEF catalog.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="catalog">The catalog to register.</param>
        /// <remarks>
        /// A simple heuristic/type scanning technique will be used to determine which MEF exports
        /// are exposed to other components in the Autofac container.
        /// </remarks>
        public static void RegisterCatalog(this ContainerBuilder builder, ComposablePartCatalog catalog)
        {
            if (builder == null) throw new ArgumentNullException("builder");
            if (catalog == null) throw new ArgumentNullException("catalog");

            RegisterCatalog(builder, catalog, ed =>
                AppDomain.CurrentDomain.GetAssemblies()
                    .Select(a => a.GetType(ed.ContractName, false))
                    .Where(t => t != null)
                    .Select(t => (Service)new TypedService(t))
                    .DefaultIfEmpty(new NamedService(ed.ContractName)));
        }

        /// <summary>
        /// Register a MEF catalog.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="catalog">The catalog to register.</param>
        /// <param name="interchangeServices">The services that will be exposed to other components in the container.</param>
        /// <remarks>
        /// Named and typed services only can be matched in the <paramref name="interchangeServices"/> collection.
        /// </remarks>
        public static void RegisterCatalog(this ContainerBuilder builder, ComposablePartCatalog catalog, params Service[] interchangeServices)
        {
            if (builder == null) throw new ArgumentNullException("builder");
            if (catalog == null) throw new ArgumentNullException("catalog");
            if (interchangeServices == null) throw new ArgumentNullException("interchangeServices");
            
            RegisterCatalog(builder, catalog, ed =>
                interchangeServices
                    .OfType<TypedService>()
                    .Where(s => ed.ContractName == CompositionServices.GetContractName(s.ServiceType))
                    .Cast<Service>()
                    .Union(
                        interchangeServices
                            .OfType<NamedService>()
                            .Where(s => ed.ContractName == s.ServiceName)
                            .Cast<Service>()
                    ));
        }

        /// <summary>
        /// Register a MEF catalog.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="catalog">The catalog to register.</param>
        /// <param name="exposedServicesMapper">A mapping function to transform ExportDefinitions into Services.</param>
        public static void RegisterCatalog(this ContainerBuilder builder, ComposablePartCatalog catalog, Func<ExportDefinition, IEnumerable<Service>> exposedServicesMapper)
        {
            if (builder == null) throw new ArgumentNullException("builder");
            if (catalog == null) throw new ArgumentNullException("catalog");
            if (exposedServicesMapper == null) throw new ArgumentNullException("exposedServicesMapper");

            foreach (var part in catalog.Parts)
            {
                RegisterPartDefinition(builder, part, exposedServicesMapper);
            }
        }

        /// <summary>
        /// Register a MEF part definition.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="part">The part definition to register.</param>
        /// <param name="exposedServicesMapper">A mapping function to transform ExportDefinitions into Services.</param>
        public static void RegisterPartDefinition(this ContainerBuilder builder, ComposablePartDefinition part, Func<ExportDefinition, IEnumerable<Service>> exposedServicesMapper)
        {
            if (builder == null) throw new ArgumentNullException("builder");
            if (part == null) throw new ArgumentNullException("part");
            if (exposedServicesMapper == null) throw new ArgumentNullException("exposedServicesMapper");

            var scope = GetInstanceScope(part);

            var partId = new UniqueService();
            builder.Register(c => part.CreatePart())
                .WithScope(scope)
                .OnActivating((sender, e) => SetPrerequisiteImports(e.Context, (ComposablePart)e.Instance))
                .OnActivated((sender, e) => SetNonPrerequisiteImports(e.Context, (ComposablePart)e.Instance))
                .As(partId);

            foreach (var exportDef in part.ExportDefinitions)
            {
                var contractService = new ContractBasedService(exportDef.ContractName);
                SupportExportCollection(builder, contractService);

                var exportId = new UniqueService();
                builder.Register(c => {
                        var p = ((ComposablePart)c.Resolve(partId));
                        return new Export(exportDef, () => p.GetExportedObject(exportDef));
                    })
                    .MemberOf(contractService)
                    .As(exportId)
                    .FactoryScoped()
                    .ExternallyOwned();

                var additionalServices = exposedServicesMapper(exportDef).ToArray();

                if (additionalServices.Length > 0)
                {
                    builder.Register(c => ((Export)c.Resolve(exportId)).GetExportedObject())
                        .As(additionalServices)
                        .FactoryScoped()
                        .ExternallyOwned();
                }
            }
        }

        /// <summary>
        /// Locate all of the MEF exports registered as supplying contract type T.
        /// </summary>
        /// <typeparam name="T">The contract type.</typeparam>
        /// <param name="context">The context to resolve exports from.</param>
        /// <returns>A list of exports.</returns>
        public static IEnumerable<Export> ResolveExports<T>(this IContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            return (IEnumerable<Export>)
                context.Resolve(new ContractBasedService(
                    CompositionServices.GetContractName(typeof(T))));
        }

        private static InstanceScope GetInstanceScope(ComposablePartDefinition part)
        {
            var scope = InstanceScope.Singleton;
            object pcp = CreationPolicy.Any;
            if (part.Metadata != null &&
                part.Metadata.TryGetValue(CompositionServices.PartCreationPolicyMetadataName, out pcp))
            {
                if (pcp != null && (CreationPolicy)pcp == CreationPolicy.NonShared)
                    scope = InstanceScope.Factory;
            }

            return scope;
        }

        private static void AttachExport(IContainer container, IComponentRegistration registration, string contractName, IDictionary<string, object> metadata)
        {
            var builder = new ContainerBuilder();
            var contractService = new ContractBasedService(contractName);

            SupportExportCollection(builder, contractService);

            builder.Register((c, p) =>
                {
                    var ctx = c.Resolve<IContext>();
                    return new Export(
                        new ExportDefinition(contractName, metadata),
                        () => ctx.Resolve(registration.Descriptor.Id));
                })
                .MemberOf(contractService)
                .ExternallyOwned()
                .ContainerScoped();

            builder.Build(container);
        }

        private static void SupportExportCollection(ContainerBuilder builder, ContractBasedService contractService)
        {
            builder.RegisterCollection<Export>().As(contractService).DefaultOnly();
        }

        private static void SetNonPrerequisiteImports(IContext context, ComposablePart composablePart)
        {
            SetImports(context, composablePart, false);
            composablePart.OnComposed();
        }

        private static void SetPrerequisiteImports(IContext context, ComposablePart composablePart)
        {
            SetImports(context, composablePart, true);
        }

        private static void SetImports(IContext context, ComposablePart composablePart, bool prerequisite)
        {
            foreach (var import in composablePart
                .ImportDefinitions
                .Where(id => id.IsPrerequisite == prerequisite))
            {
                var cbid = import as ContractBasedImportDefinition;
                if (cbid == null)
                    throw new NotSupportedException(string.Format(MefExtensionsResources.ContractBasedOnly, import));

                var eds = new ContractBasedService(cbid.ContractName);

                ICollection<Export> exportsForContract;
                if (context.IsRegistered(eds))
                {
                    var rm = cbid.RequiredMetadata ?? Enumerable.Empty<string>();
                    exportsForContract = ((ICollection<Export>)context.Resolve(eds))
                        .Where(ex => !cbid.RequiredMetadata.Except(ex.Metadata.Keys).Any())
                        .ToList();
                }
                else
                {
                    exportsForContract = new Export[0];
                }

                if (cbid.Cardinality == ImportCardinality.ExactlyOne && exportsForContract.Count == 0)
                    throw new ComponentNotRegisteredException(eds);

                composablePart.SetImport(import, exportsForContract);
            }
        }
    }
}
