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
    public static class MefExtensions
    {
        static IDictionary<string, object> EmptyMetadata = new Dictionary<string, object>();
            
        public static IConcreteRegistrar ExportedAs(this IConcreteRegistrar registrar, string contractName)
        {
            return ExportedAs(registrar, contractName, EmptyMetadata);
        }

        public static IConcreteRegistrar ExportedAs(this IConcreteRegistrar registrar, string contractName, IDictionary<string, object> metadata)
        {
            if (registrar == null)
                throw new ArgumentNullException("registrar");

            if (contractName == null)
                throw new ArgumentNullException("contractName");

            if (metadata == null)
                throw new ArgumentNullException("metadata");

            registrar.OnRegistered((s, e) => AttachExport(e.Container, e.Registration, contractName, metadata));

            return registrar;
        }

        public static IConcreteRegistrar ExportedAs(this IConcreteRegistrar registrar, Type contractType)
        {
            return ExportedAs(registrar, contractType, EmptyMetadata);
        }

        public static IConcreteRegistrar ExportedAs<ContractType>(this IConcreteRegistrar registrar)
        {
            return ExportedAs(registrar, typeof(ContractType), EmptyMetadata);
        }

        public static IConcreteRegistrar ExportedAs(this IConcreteRegistrar registrar, Type contractType, IDictionary<string, object> metadata)
        {
            return ExportedAs(registrar, CompositionServices.GetContractName(contractType), metadata);
        }

        public static IConcreteRegistrar ExportedAs<ContractType>(this IConcreteRegistrar registrar, IDictionary<string, object> metadata)
        {
            return ExportedAs(registrar, typeof(ContractType), metadata);
        }

        public static IReflectiveRegistrar ExportedAs(this IReflectiveRegistrar registrar, string contractName)
        {
            return ExportedAs(registrar, contractName, EmptyMetadata);
        }

        public static IReflectiveRegistrar ExportedAs(this IReflectiveRegistrar registrar, string contractName, IDictionary<string, object> metadata)
        {
            if (registrar == null)
                throw new ArgumentNullException("registrar");

            if (contractName == null)
                throw new ArgumentNullException("contractName");

            if (metadata == null)
                throw new ArgumentNullException("metadata");

            registrar.OnRegistered((s, e) => AttachExport(e.Container, e.Registration, contractName, metadata));

            return registrar;
        }

        public static IReflectiveRegistrar ExportedAs(this IReflectiveRegistrar registrar, Type contractType)
        {
            return ExportedAs(registrar, contractType, EmptyMetadata);
        }

        public static IReflectiveRegistrar ExportedAs<ContractType>(this IReflectiveRegistrar registrar)
        {
            return ExportedAs(registrar, typeof(ContractType), EmptyMetadata);
        }

        public static IReflectiveRegistrar ExportedAs(this IReflectiveRegistrar registrar, Type contractType, IDictionary<string, object> metadata)
        {
            return ExportedAs(registrar, CompositionServices.GetContractName(contractType), metadata);
        }

        public static IReflectiveRegistrar ExportedAs<ContractType>(this IReflectiveRegistrar registrar, IDictionary<string, object> metadata)
        {
            return ExportedAs(registrar, typeof(ContractType), metadata);
        }

        public static void RegisterCatalog(this ContainerBuilder builder, ComposablePartCatalog catalog)
        {
            RegisterCatalog(builder, catalog, ed =>
                AppDomain.CurrentDomain.GetAssemblies()
                    .Select(a => a.GetType(ed.ContractName, false))
                    .Where(t => t != null)
                    .Select(t => (Service)new TypedService(t))
                    .DefaultIfEmpty(new NamedService(ed.ContractName)));
        }

        public static void RegisterCatalog(this ContainerBuilder builder, ComposablePartCatalog catalog, Func<ExportDefinition, IEnumerable<Service>> exposedServicesMapper)
        {
            foreach (var part in catalog.Parts)
            {
                RegisterPartDefinition(builder, part, exposedServicesMapper);
            }
        }

        public static void RegisterPartDefinition(this ContainerBuilder builder, ComposablePartDefinition part, Func<ExportDefinition, IEnumerable<Service>> nativeServiceMapper)
        {
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

                var additionalServices = nativeServiceMapper(exportDef).ToArray();

                if (additionalServices.Length > 0)
                {
                    builder.Register(c => ((Export)c.Resolve(exportId)).GetExportedObject())
                        .As(additionalServices)
                        .FactoryScoped()
                        .ExternallyOwned();
                }
            }
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
