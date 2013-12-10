namespace Autofac.Core.Registration
{
    /// <summary>
    /// Interface providing fluent syntax for chaining module registrations.
    /// </summary>
    public interface IModuleRegistrar
    {
        /// <summary>
        /// Add a module to the container.
        /// </summary>
        /// <param name="module">The module to add.</param>
        /// <returns>
        /// The <see cref="Autofac.Core.Registration.IModuleRegistrar"/> to allow
        /// additional chained module registrations.
        /// </returns>
        IModuleRegistrar RegisterModule(IModule module);
    }
}
