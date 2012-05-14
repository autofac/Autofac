using System;
using Autofac;
using Autofac.Builder;

namespace AutofacContrib.Attributed
{
    public interface IMetadataRegistrar<in TInterface, in TMetadata>
    {
        IRegistrationBuilder<TInstance, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterType
            <TInstance>(TMetadata metadata) where TInstance : TInterface;

        IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterType
            (Type instanceType, TMetadata metadata);


        IRegistrationBuilder<TInstance, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterAttributedType
            <TInstance>() where TInstance : TInterface;

        IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterAttributedType
            (Type instanceType);

        ContainerBuilder ContainerBuilder { get; }

    }


    /// <summary>
    /// provides a mechanism to separate metadata registrations from compile-time attributes
    /// </summary>
    /// <typeparam name="TInterface">interface used on concrete types of metadata decorated instances</typeparam>
    /// <typeparam name="TMetadata">strongly typed metadata definition</typeparam>
    public abstract class MetadataModule<TInterface, TMetadata> : Module, IMetadataRegistrar<TInterface, TMetadata>
    {
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
            ContainerBuilder = builder;
            Register(this);
        }

        #region IMetadataRegistrar<TInterface,TMetadata> Members

        /// <summary>
        /// registers provided metadata on the declared type 
        /// </summary>
        /// <typeparam name="TInstance">concrete instance type</typeparam>
        /// <param name="metadata">metadata instance</param>
        /// <returns>container builder</returns>
        public IRegistrationBuilder<TInstance, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterType<TInstance>(TMetadata metadata) where TInstance : TInterface
        {
            return
                ContainerBuilder.RegisterType<TInstance>().As<TInterface>().WithMetadata(
                    MetadataHelper.GetProperties(metadata));
        }


        /// <summary>
        /// registers the provided concrete instance and scans it for generic MetadataAttribute data
        /// </summary>
        /// <typeparam name="TInstance">concrete instance type</typeparam>
        /// <returns></returns>
        public IRegistrationBuilder<TInstance, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterAttributedType<TInstance>() where TInstance : TInterface
        {
            return
                ContainerBuilder.RegisterType<TInstance>().As<TInterface>().WithMetadata(
                    MetadataHelper.GetMetadata(typeof(TInstance)));
        }

        /// <summary>
        /// registers provided metadata on the declared type
        /// </summary>
        /// <param name="instanceType">Type of the instance.</param>
        /// <param name="metadata">The metadata.</param>
        /// <returns>registration builder</returns>
        public IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterType(Type instanceType, TMetadata metadata)
        {
            return
                ContainerBuilder.RegisterType(instanceType).As<TInterface>().WithMetadata(
                    MetadataHelper.GetProperties(metadata));
        }

        /// <summary>
        /// registers the provided concrrete instance type and scans it for generate metadata data
        /// </summary>
        /// <param name="instanceType">Type of the instance.</param>
        /// <returns></returns>
        public IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterAttributedType(Type instanceType)
        {
            return
                ContainerBuilder.RegisterType(instanceType).As<TInterface>().WithMetadata(
                    MetadataHelper.GetMetadata(instanceType));
        }

        public ContainerBuilder ContainerBuilder { get; private set; }

        #endregion
    }
}
