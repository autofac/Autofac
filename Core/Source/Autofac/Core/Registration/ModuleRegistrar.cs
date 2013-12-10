using System;

namespace Autofac.Core.Registration
{
    /// <summary>
    /// Basic implementation of the <see cref="Autofac.Core.Registration.IModuleRegistrar"/>
    /// interface allowing registration of modules into a <see cref="Autofac.ContainerBuilder"/>
    /// in a fluent format.
    /// </summary>
    internal class ModuleRegistrar : IModuleRegistrar
    {
        /// <summary>
        /// The <see cref="Autofac.ContainerBuilder"/> into which registrations will be made.
        /// </summary>
        private ContainerBuilder _builder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleRegistrar"/> class.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="Autofac.ContainerBuilder"/> into which registrations will be made.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="builder" /> is <see langword="null" />.
        /// </exception>
        public ModuleRegistrar(ContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            this._builder = builder;
        }

        /// <summary>
        /// Add a module to the container.
        /// </summary>
        /// <param name="module">The module to add.</param>
        /// <returns>
        /// The <see cref="Autofac.Core.Registration.IModuleRegistrar"/> to allow
        /// additional chained module registrations.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="module" /> is <see langword="null" />.
        /// </exception>
        public IModuleRegistrar RegisterModule(IModule module)
        {
            if (module == null)
            {
                throw new ArgumentNullException("module");
            }

            this._builder.RegisterCallback(module.Configure);
            return this;
        }
    }
}
