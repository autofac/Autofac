using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.Linq;

namespace AutofacContrib.Attributed.MEF
{
    /// <summary>
    /// this container override exists to provide a mapping between the Lazy instance and the associated instantiation type
    /// 
    /// TODO: this feels like an custom exporter in MEF, but need to better understand the GetExports internals
    /// </summary>
    internal class ExportInfoCompositionContainer : CompositionContainer
    {
        private IEnumerable<Export> _exports;

        internal ExportInfoCompositionContainer(ComposablePartCatalog catalog, params ExportProvider[] providers) : base(catalog, providers) { }

        /// <summary>
        /// override the underlyiing get exports core call to hang onto the extra data received in this call
        /// </summary>
        /// <param name="definition">import definition</param>
        /// <param name="atomicComposition">compositional operation scope</param>
        /// <returns>list of exports</returns>
        protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
        {
            _exports = base.GetExportsCore(definition, atomicComposition);
            return _exports;
        }

        /// <summary>
        /// performs a GetExports command and enumerates the map of the lazy and metadata to the instantiation type
        /// </summary>
        /// <typeparam name="TInterface">interface being scanned for</typeparam>
        /// <typeparam name="TMetadata">strong metadata type</typeparam>
        /// <returns>enumerable of extended export information</returns>
        internal IEnumerable<ExtendedExportInfo<TInterface, TMetadata>> GetExportsWithTargetType<TInterface, TMetadata>()
        {
            var results = GetExports<TInterface, TMetadata>();

            var currentPosition = 0;
            var exportList = _exports.ToList();

            return results.Select(result => new ExtendedExportInfo<TInterface, TMetadata>(ReflectionModelServices.GetExportingMember(exportList[currentPosition++].Definition).GetAccessors()[0] as Type, result));
        }
    }
}