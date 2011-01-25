namespace Autofac.Core.Diagnostics
{
    /// <summary>
    /// Marks a module as container-aware (for the purposes of attaching to diagnostic events.)
    /// </summary>
    public interface IContainerAwareComponent
    {
        /// <summary>
        /// Initialise the module with the container into which it is being registered.
        /// </summary>
        /// <param name="container">The container.</param>
        void SetContainer(IContainer container);
    }
}
