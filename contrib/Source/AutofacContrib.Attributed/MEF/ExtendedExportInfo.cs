using System;

namespace AutofacContrib.Attributed.MEF
{
    /// <summary>
    /// this is an internal class responsible for holding a concrete type 
    /// reference and the associated lazy instantiator and its associated
    /// metadata info.
    /// </summary>
    /// <typeparam name="TInterface">interface being searched for, the instantiation type will be derived from this type</typeparam>
    /// <typeparam name="TMetadata">strongly typed description of the metadata</typeparam>
    internal class ExtendedExportInfo<TInterface, TMetadata>
    {
        /// <summary>
        /// a lazy reference to an instance of type TInterface and its associated metadata
        /// </summary>
        public Lazy<TInterface, TMetadata> Value { get; private set; }

        /// <summary>
        /// the conrete instantiation type that is, or is derived from, type TInterface
        /// </summary>
        public Type InstantiationType { get; private set; }

        /// <summary>
        /// standard ctor
        /// </summary>
        /// <param name="instantiationType">concrete instatiation type</param>
        /// <param name="value">lazy instantiation info along with metadata</param>
        /// <exception cref="ArgumentNullException">made when either of the types is null</exception>
        public ExtendedExportInfo(Type instantiationType, Lazy<TInterface, TMetadata> value)
        {
            if (instantiationType == null)
                throw new ArgumentNullException("instantiationType");

            if (value == null)
                throw new ArgumentNullException("value");

            InstantiationType = instantiationType;
            Value = value;
        }
    }
}
