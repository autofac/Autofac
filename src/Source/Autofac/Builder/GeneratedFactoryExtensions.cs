using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.GeneratedFactories;
using Autofac.Registrars;

namespace Autofac.Builder
{
    /// <summary>
    /// Extends ContainerBuilder with methods for registering generated factories.
    /// </summary>
    /// <remarks>
    /// This file is excluded from the .NET 2.0 build.
    /// </remarks>
    public static class GeneratedFactoryExtensions
    {
        /// <summary>
        /// Registers the factory delegate.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <typeparam name="TDelegate">The type of the delegate.</typeparam>
        /// <param name="service">The service that the delegate will return instances of.</param>
        /// <returns>A registrar allowing configuration to continue.</returns>
        public static IConcreteRegistrar RegisterGeneratedFactory<TDelegate>(this ContainerBuilder builder, Service service)
            where TDelegate : class
        {
            Enforce.ArgumentNotNull(builder, "builder");
            Enforce.ArgumentNotNull(service, "service");

            var factory = new FactoryGenerator<TDelegate>(service);

            return builder.Register<TDelegate>((c, p) => factory.GenerateFactory(c, p))
                .ContainerScoped();
        }
        /// <summary>
        /// Registers the factory delegate.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <typeparam name="TDelegate">The type of the delegate.</typeparam>
        /// <returns>A registrar allowing configuration to continue.</returns>
        public static IConcreteRegistrar RegisterGeneratedFactory<TDelegate>(this ContainerBuilder builder)
            where TDelegate : class
        {
            Enforce.ArgumentNotNull(builder, "builder");
            Enforce.ArgumentTypeIsFunction(typeof(TDelegate));

            var returnType = typeof(TDelegate).GetMethod("Invoke").ReturnType;
            return builder.RegisterGeneratedFactory<TDelegate>(new TypedService(returnType));
        }
    }
}
