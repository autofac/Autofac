using System.Collections.Generic;
using Autofac;
using Autofac.Builder;

namespace AutofacContrib.Attributed
{
    public interface IMetadataRegistrar<TInterface, TMetadata>
    {
        IRegistrationBuilder<TInstance, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterTypedMetadata
            <TInstance>(IEnumerable<TMetadata> metadataSet) where TInstance : TInterface;
    }


    /// <summary>
    /// provides a mechanism to separate metadata registrations from compile-time attributes
    /// </summary>
    /// <typeparam name="TInterface">interface used on concrete types of metadata decorated instances</typeparam>
    /// <typeparam name="TMetadata">strongly typed metadata definition</typeparam>
    public abstract class MetadataModule<TInterface, TMetadata> : Module, IMetadataRegistrar<TInterface, TMetadata>
    {
        private ContainerBuilder _containerBuilder;

        /// <summary>
        /// client overrided method where metadata registration is performed
        /// </summary>
        /// <param name="registrar">wrapped metadata registry interface</param>
        public abstract void Register(IMetadataRegistrar<TInterface, TMetadata> registrar);

        /// <summary>
        /// standard module method being overloaded and sealed to provide wrapped metadata registration
        /// </summary>
        /// <param name="builder">container builder</param>
        sealed protected override void Load(ContainerBuilder builder)
        {
            _containerBuilder = builder;
            Register(this);
        }

        #region IMetadataRegistrar<TInterface,TMetadata> Members

        /// <summary>
        /// registrar providing 1 or more metadata registrations for a given concrete type
        /// </summary>
        /// <typeparam name="TInstance">concrete instance type</typeparam>
        /// <param name="metadataSet">enumeration of associated metadata types to be registered</param>
        /// <returns>registration builder to further configure the concrete type registration such as lifetime</returns>
        public IRegistrationBuilder<TInstance, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterTypedMetadata<TInstance>(IEnumerable<TMetadata> metadataSet) where TInstance : TInterface
        {
            return _containerBuilder.RegisterTypedMetadata<TInterface, TInstance, TMetadata>(metadataSet);
        }

        #endregion
    }
}
