using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Builder;

namespace AutofacContrib.Attributed
{
    public interface IMetadataRegistrar<TInterface, TMetadata>
    {
        IRegistrationBuilder<TInstance, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterType
            <TInstance>(TMetadata metadata) where TInstance : TInterface;

        IRegistrationBuilder<TInstance, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterAttributedType
            <TInstance>(TMetadata metadata) where TInstance : TInterface;
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

        private static IEnumerable<KeyValuePair<string, object>> TranslateMetadata(object instance)
        {
            return instance.GetType().GetProperties().Where(propertyInfo => propertyInfo.CanRead)
                            .Select(propertyInfo => new KeyValuePair<string, object>
                                            (propertyInfo.Name, propertyInfo.GetValue(instance, null))).ToDictionary(pair => pair.Key, pair => pair.Value);
        }


        #region IMetadataRegistrar<TInterface,TMetadata> Members



        public IRegistrationBuilder<TInstance, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterType<TInstance>(TMetadata metadata) where TInstance : TInterface
        {
            return _containerBuilder.RegisterType<TInstance>().As<TInterface>().WithMetadata(TranslateMetadata(metadata));
        }



        public IRegistrationBuilder<TInstance, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterAttributedType<TInstance>(TMetadata metadata) where TInstance : TInterface
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
