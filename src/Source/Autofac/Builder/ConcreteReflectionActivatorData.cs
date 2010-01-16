using System;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;

namespace Autofac.Builder
{
    /// <summary>
    /// Reflection activator data for concrete types.
    /// </summary>
    public class ConcreteReflectionActivatorData : ReflectionActivatorData, IConcreteActivatorData
    {
        /// <summary>
        /// Specify a reflection activator for the given type.
        /// </summary>
        /// <param name="implementor">Type that will be activated.</param>
        public ConcreteReflectionActivatorData(Type implementor)
            : base(implementor)
        {
        }

        /// <summary>
        /// The instance activator based on the provided data.
        /// </summary>
        public IInstanceActivator Activator
        {
            get
            {
                return new ReflectionActivator(
                    ImplementationType,
                    ConstructorFinder,
                    ConstructorSelector,
                    ConfiguredParameters,
                    ConfiguredProperties);
            }
        }
    }
}
